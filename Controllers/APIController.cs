﻿using CarCareTracker.External.Implementations;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class APIController : Controller
    {
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IFileHelper _fileHelper;
        public APIController(IVehicleDataAccess dataAccess,
            IGasHelper gasHelper,
            IReminderHelper reminderHelper,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IFileHelper fileHelper,
            IUserLogic userLogic) 
        {
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _gasHelper = gasHelper;
            _reminderHelper = reminderHelper;
            _userLogic = userLogic;
            _fileHelper = fileHelper;
        }
        public IActionResult Index()
        {
            return View();
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        [HttpGet]
        [Route("/api/vehicles")]
        public IActionResult Vehicles()
        {
            var result = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                result = _userLogic.FilterUserVehicles(result, GetUserID());
            }
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/servicerecords")]
        public IActionResult ServiceRecords(int vehicleId)
        {
            var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/repairrecords")]
        public IActionResult RepairRecords(int vehicleId)
        {
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/upgraderecords")]
        public IActionResult UpgradeRecords(int vehicleId)
        {
            var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/taxrecords")]
        public IActionResult TaxRecords(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/odometerrecords")]
        public IActionResult OdometerRecords(int vehicleId)
        {
            var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { Date = x.Date.ToShortDateString(), Odometer = x.Mileage.ToString(), Notes = x.Notes });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        public IActionResult AddOdometerRecord(int vehicleId, OdometerRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                return Json(response);
            }
            try
            {
                var odometerRecord = new OdometerRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Mileage = int.Parse(input.Odometer)
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                response.Success = true;
                response.Message = "Odometer Record Added";
                return Json(response);
            } catch (Exception ex)
            {
                response.Success = false;
                response.Message = StaticHelper.GenericErrorMessage;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, bool useMPG, bool useUKMPG)
        {
            var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var result = _gasHelper.GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG)
                .Select(x => new GasRecordExportModel { 
                    Date = x.Date, 
                    Odometer = x.Mileage.ToString(), 
                    Cost = x.Cost.ToString(), 
                    FuelConsumed = x.Gallons.ToString(), 
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
                    Notes = x.Notes
                });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/reminders")]
        public IActionResult Reminders(int vehicleId)
        {
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x=> new ReminderExportModel {  Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes});
            return Json(results);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/makebackup")]
        public IActionResult MakeBackup()
        {
            var result = _fileHelper.MakeBackup();
            return Json(result);
        }
        private int GetMaxMileage(int vehicleId)
        {
            var numbersArray = new List<int>();
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            if (serviceRecords.Any())
            {
                numbersArray.Add(serviceRecords.Max(x => x.Mileage));
            }
            var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            if (repairRecords.Any())
            {
                numbersArray.Add(repairRecords.Max(x => x.Mileage));
            }
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            if (gasRecords.Any())
            {
                numbersArray.Add(gasRecords.Max(x => x.Mileage));
            }
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            if (upgradeRecords.Any())
            {
                numbersArray.Add(upgradeRecords.Max(x => x.Mileage));
            }
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            if (odometerRecords.Any())
            {
                numbersArray.Add(odometerRecords.Max(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
    }
}
