﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ConsultantRow : SQLBaseClass
    {
        private int offer_number;
        public int Offer_number { get { return offer_number; } set { offer_number = value; } }

        private int code;
        public int Code { get { return code; } set { code = value; } }

        private int? amount;
        public int? Amount { get { return amount; } set { amount = value; } }

        private decimal? total_price;
        public decimal? Total_price { get { return total_price; } set { total_price = value; } }

        private bool? include_status;
        public bool? Include_status { get { return include_status; } set { include_status = value; } }

        private String alias;
        public String Alias { get { return alias; } set { alias = value; } }

        private long ssma_timestamp;
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        /// <summary>
        /// Offer description for module, needed for new dialog "Edit Module Info". Not saved to database.
        /// </summary>
        public string _OfferDescription { get; set; }

        /// <summary>
        /// Id for ModuleText, not in DB 
        /// </summary>
        public int _ModuleTextId { get; set; }

        public view_ConsultantRow()
            : base("ConsultantRow")
        {
            //ctr
        }

        /// <summary>
        /// Compare two consultant rows depending on their code values.
        /// </summary>
        /// <param name="obj">The other consultant row</param>
        /// <returns>true/false depending on compare result.</returns>
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_ConsultantRow p = obj as view_ConsultantRow;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Code == p.Code);
        }

        /// <summary>
        /// Gets all consultant rows with a specific offerNumber
        /// </summary>
        /// <param name="offerNumber">The offer number to get from</param>
        /// <returns>A list of consultant rows.</returns>
        public static List<view_ConsultantRow> getAllConsultantRow(String offerNumber)
        {
            List<view_ConsultantRow> list = new List<view_ConsultantRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = "SELECT Offer_number ,Code ,Amount ,Total_price ,Include_status, Alias, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ConsultantRow WHERE " + "Offer_number = @offerNumber";

                command.Prepare();
                command.Parameters.AddWithValue("@offerNumber", offerNumber);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ConsultantRow t = new view_ConsultantRow();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }

                            //Also get Offer Description (ModuleText)
                            view_Module service = new view_Module();
                            service.Select("Article_number = " + t.Code.ToString());
                            view_ModuleText moduleText = new view_ModuleText();
                            moduleText.Select("Type = 'O' AND TypeId = " + t.Offer_number.ToString() + " AND ModuleType = 'T' AND ModuleId = " + t.Code.ToString());

                            t._OfferDescription = moduleText.Description;
                            t._ModuleTextId = moduleText._ID;

                            list.Add(t);
                        }
                    }
                }
            }

            return list;
        }

         /// <summary>
         /// Gets all consultant rows
         /// </summary>
         /// <returns>A list of all consultant rows.</returns>
        public static List<view_ConsultantRow> getAllConsultantRow()
        {
            List<view_ConsultantRow> list = new List<view_ConsultantRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Offer_number ,Code ,Amount ,Total_price ,Include_status, Alias, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ConsultantRow";

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ConsultantRow t = new view_ConsultantRow();
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


            }
            return list;
        }
    }

}