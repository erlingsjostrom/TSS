﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ModuleDiscount : SQLBaseClass
    {
        private int _id;
        public int _ID { get; set; }

        private int article_number;
        public int Article_number { get; set; }

        private String area;
        public String Area { get; set; }

        private DateTime start_date;
        public DateTime Start_date { get; set; }

        private DateTime end_date;
        public DateTime End_date { get; set; }

        private int discount;
        public int Discount { get; set; }

        public view_ModuleDiscount() : base("ModuleDiscount")
        {

        }

        public static List<view_ModuleDiscount> getAllModuleDiscounts(String area)
        {
            List<view_ModuleDiscount> list = new List<view_ModuleDiscount>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = "SELECT * FROM " + databasePrefix + "ModuleDiscount";

                if (!String.IsNullOrEmpty(area))
                    query += " WHERE Area=@area";

                SqlCommand command = new SqlCommand(query, connection);
                command.Prepare();

                if (!String.IsNullOrEmpty(area))
                    command.Parameters.AddWithValue("@area", area);
                
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_ModuleDiscount t = new view_ModuleDiscount();
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public static List<view_ModuleDiscount> getAllModuleDiscounts()
        {
            return getAllModuleDiscounts(null);
        }
    }
}