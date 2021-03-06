﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;

namespace TietoCRM.Models
{
    public class view_CustomerMissingProductReport : SQLBaseClass
    {
        private String customer;
        public String Customer { get; set; }

        private int article_number;
        public int Article_number { get; set; }

        private String module;
        public String Module { get; set; }

        private String system;
        public String System { get; set; }

        private String classification;
        public String Classification { get; set; }

        private String status;
        public String Status { get; set; }

        private String sign;
        public String Sign { get { return sign; } set { sign = value; } }

        private int license;
        public int License { get; set; }

        private int maintenance;
        public int Maintenance { get; set; }

        private decimal fixed_price;
        public decimal Fixed_price { get; set; }

        private String area;
        public String Area { get; set; }

        private decimal sortNo;
        public decimal SortNo { get { return sortNo; } set { sortNo = value; } }

        public view_CustomerMissingProductReport()
            : base("CustomerMissingProductReport")
        {
            //ctr
        }
        /// <summary>
        /// Gets all the product rows
        /// </summary>
        /// <returns>A list of product rows.</returns>
        public static List<view_CustomerMissingProductReport> getCustomerMissingProducts(string customer, string area)
        {

            List<view_CustomerMissingProductReport> list = new List<view_CustomerMissingProductReport>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //String query = "SELECT * FROM " + databasePrefix + "CustomerProductsMissing Where Customer = @customer Order By Fixed_price, classification, status, module";
                String query = "stp_MissingProducts";

                SqlCommand command = new SqlCommand(query, connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Prepare();
                command.Parameters.AddWithValue("@pCustomer", customer);
                command.Parameters.AddWithValue("@pArea", area);
                //command.Parameters.AddWithValue("@area", user.Area);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerMissingProductReport t = new view_CustomerMissingProductReport();
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
                connection.Close();
            }
            return list;
        }

        public static DataTable ExportCustomerMissingProductsToExcel(string customer, string area)
        {
            DataTable dt = new DataTable(customer.Replace(" ", "_"));
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                String query = "stp_MissingProducts";

                SqlCommand command = new SqlCommand(query, connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Prepare();
                command.Parameters.AddWithValue("@pCustomer", customer);
                command.Parameters.AddWithValue("@pArea", area);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                }
                connection.Close();
            }
            return dt;
        }
    }
}