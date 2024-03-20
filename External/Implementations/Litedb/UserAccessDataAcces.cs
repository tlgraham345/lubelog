﻿using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UserAccessDataAccess : IUserAccessDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "useraccessrecords";
        public UserAccessDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        /// <summary>
        /// Gets a list of vehicles user have access to.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserAccess> GetUserAccessByUserId(int userId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            return table.Find(x => x.Id.UserId == userId).ToList();
        }
        public UserAccess GetUserAccessByVehicleAndUserId(int userId, int vehicleId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            return table.Find(x => x.Id.UserId == userId && x.Id.VehicleId == vehicleId).FirstOrDefault();
        }
        public List<UserAccess> GetUserAccessByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            return table.Find(x => x.Id.VehicleId == vehicleId).ToList();
        }
        public bool SaveUserAccess(UserAccess userAccess)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            table.Upsert(userAccess);
            db.Checkpoint();
            return true;
        }
        public bool DeleteUserAccess(int userId, int vehicleId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            table.DeleteMany(x => x.Id.UserId == userId && x.Id.VehicleId == vehicleId);
            db.Checkpoint();
            return true;
        }
        /// <summary>
        /// Delete all access records when a vehicle is deleted.
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            table.DeleteMany(x => x.Id.VehicleId == vehicleId);
            db.Checkpoint();
            return true;
        }
        /// <summary>
        /// Delee all access records when a user is deleted.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByUserId(int userId)
        {
            var table = db.GetCollection<UserAccess>(tableName);
            table.DeleteMany(x => x.Id.UserId == userId);
            db.Checkpoint();
            return true;
        }
    }
}