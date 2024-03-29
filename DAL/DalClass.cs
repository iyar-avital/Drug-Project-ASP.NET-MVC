﻿using BE;
using DAL.ImaggaHelperClass;
using DAL.InteractionHelperClass;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DAL
{
    public class DalClass : IDAL

    {

        #region ADD
        public void AddDoctor(Doctor doctor)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Doctors.Add(doctor);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddMedicine(Medicine medicine, HttpPostedFileBase httpPostedFile)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Medicines.Add(medicine);
                    ctx.SaveChanges();
                }
                GoogleDriveAPITool.FileUpload(httpPostedFile);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddPatient(Patient patient)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Patients.Add(patient);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddPrescription(Prescription prescription)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Prescriptions.Add(prescription);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region DELETE
        public bool DeleteMedicine(int? id)
        {

            bool Result = true;
            try
            {
                Medicine medicine = GetMedicine(id);
                if (medicine == null)
                    return false;
                using (var ctx = new DrugsContext())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.Medicines.Attach(medicine);
                    ctx.Entry(medicine).State = EntityState.Deleted;
                    ctx.SaveChanges();          
                }
                GoogleDriveAPITool.DeleteGoogleFileByName(medicine.imagePath);

            }
            catch (Exception)
            {

                Result = false;
            }

            return Result;
        }

        public bool DeletePatient(int? id)
        {
            bool Result = true;
            try
            {
                Patient patient = GetPatient(id);
                if (patient == null)
                    return false;
                using (var ctx = new DrugsContext())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.Patients.Attach(patient);
                    ctx.Entry(patient).State = EntityState.Deleted;
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {

                Result = false;
            }

            return Result;
        }

        public bool DeleteDoctor(int? id)
        {
            bool Result = true;
            try
            {
                Doctor doctor = GetDoctor(id);
                if (doctor == null)
                    return false;
                using (var ctx = new DrugsContext())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.Doctors.Attach(doctor);
                    ctx.Entry(doctor).State = EntityState.Deleted;
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {

                Result = false;
            }

            return Result;
        }
        #endregion

        #region GET LISTS
        public IEnumerable<Doctor> GetDoctors(Func<Doctor, bool> predicat = null)
        {            
            using (var ctx = new DrugsContext())
            {
                if (predicat == null)
                    return ctx.Doctors.ToList();

                var doc = ctx.Doctors.Where(predicat).ToList();
                return doc;
            }
        }

        public IEnumerable<Medicine> GetMedicines(Func<Medicine, bool> predicat = null)
        {
            using (var ctx = new DrugsContext())
            {
                if (predicat == null)
                    return ctx.Medicines.ToList();

                var med = ctx.Medicines.Where(predicat).ToList();
                return med;
            }
        }

        public IEnumerable<Patient> GetPatients(Func<Patient, bool> predicat = null)
        {
            using (var ctx = new DrugsContext())
            {
                if (predicat == null)
                    return ctx.Patients.ToList();

                var patien = ctx.Patients.Where(predicat).ToList();
                return patien;
            }
        }

        public IEnumerable<Prescription> GetPrescriptions(Func<Prescription, bool> predicat = null)
        {
            using(var ctx = new DrugsContext())
            {
                if (predicat == null)
                    return ctx.Prescriptions.ToList();


                var pres = ctx.Prescriptions.Where(predicat).ToList();
                return pres;
            }  
        }

        public IEnumerable<MedicineWrraper> GetAllNDC()
        {
            using (var ctx = new DrugsContext())
            {
                  return ctx.Name_NDC.ToList();
            }
        }

        #endregion

        #region UPDATE
        public void UpdateMedicine(Medicine medicine, HttpPostedFileBase httpPostedFile)
        {
            try
            {
                Medicine med = GetMedicine(medicine.Id);
                GoogleDriveAPITool.DeleteGoogleFileByName(med.imagePath);
                using (var ctx = new DrugsContext())
                {
                    ctx.Entry(medicine).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
                GoogleDriveAPITool.FileUpload(httpPostedFile);             
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void UpdatePatient(Patient patient)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Entry(patient).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateDoctor(Doctor doctor)
        {
            try
            {
                using (var ctx = new DrugsContext())
                {
                    ctx.Entry(doctor).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region GET
        public Doctor GetDoctor(int? id)
        {
            using (var ctx = new DrugsContext())
            {
                return ctx.Doctors.Find(id); 
            }
        }

        public Medicine GetMedicine(int? id)
        {
            using (var ctx = new DrugsContext())
            {
                return ctx.Medicines.Find(id); 
            }
        }

        public Patient GetPatient(int? id)
        {
            using (var ctx = new DrugsContext())
            {
                return ctx.Patients.Find(id); 
            }
        }

        public Prescription GetPrescription(int? id)
        {
            using (var ctx = new DrugsContext())
            {
                return ctx.Prescriptions.Find(id); 
            }
        }
        public string GetNDCForMedicine(string genericName)
        {
            using (var ctx = new DrugsContext())
            {
                return ctx.Name_NDC.Where(med => med.genericName == genericName).Select(med => med.NDCnumber).FirstOrDefault(); 
            }
        }
        #endregion

        #region IMAGE SERVICE
        public void GetTags(ImageDetails CurrentImage)
        {
            //POST request using RestSharp
            string apiKey = "acc_fb83cd5b0478486";
            string apiSecret = "d5dc3c0c6b26d4cd2ce752073d3ddd07";
            string image = CurrentImage.ImagePath;

            string basicAuthValue = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", apiKey, apiSecret)));

            var client = new RestClient("https://api.imagga.com/v2/tags");
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", String.Format("Basic {0}", basicAuthValue));
            request.AddFile("image", image);

            IRestResponse response = client.Execute(request);

            Root TagList = JsonConvert.DeserializeObject<Root>(response.Content); //deserialization for the json response
            //root->result->tags->tag->tag, confidence
            foreach (var item in TagList.result.tags)//going on all the results
            {
                CurrentImage.Details.Add(item.tag.en, item.confidence);
            }
        }
        #endregion

        #region  CONFLICT DRUGS SERVICE 
        public List<string> IsConflict(List<string> NDC)

        {
            WebClient webC = new WebClient();

            //finding the RXCUI code by the NDC code
            List<string> RXCUI = new List<string>();
            string linkXML, XML, jsonText, RXCUICode;
            XmlDocument document;

            for (int i = 0; i < NDC.Count(); i++)
            {
                //the URL link to the XML file includeing the RXCUI code
                linkXML = @"https://rxnav.nlm.nih.gov/REST/rxcui?idtype=NDC&id=" + NDC[i];

                //downloading the files from the server
                XML = webC.DownloadString(linkXML);
                document = new XmlDocument();
                document.LoadXml(XML);

                //convert the xml to json file
                jsonText = JsonConvert.SerializeXmlNode(document);
                Root1 jsonClass = JsonConvert.DeserializeObject<Root1>(jsonText);

                try
                {
                    RXCUICode = jsonClass.rxnormdata.idGroup.rxnormId;
                }
                catch (Exception)
                {
                    RXCUICode = "false";
                }
                RXCUI.Add(RXCUICode);
            }

            //if there is no Rx code
            if (RXCUI.All(IsFalse) == true)
            {
                return new List<string>() { "", "false" };
            }

            //the URL link to the XML file includeing interaction result
            string linkIteraction = "https://rxnav.nlm.nih.gov/REST/interaction/list?rxcuis=" + RXCUI[0];

            for (int i = 1; i < RXCUI.Count(); i++)
            {
                linkIteraction = linkIteraction + "+" + RXCUI[i];
            }

            List<string> strings = Isinterected(linkIteraction);
            return strings;
        }

        public List<string> Isinterected(string linkIteraction)
        {
            WebClient webC = new WebClient();

            string XML = webC.DownloadString(linkIteraction);
            XmlDocument document = new XmlDocument();
            document.LoadXml(XML);
            string jsonText = JsonConvert.SerializeXmlNode(document);

            string Comment = "";
            List<InteractionPair> Pair = null;
            List<string> Severity = new List<string>();

            try
            {
                Root2 jsonClass = JsonConvert.DeserializeObject<Root2>(jsonText);
                //Comment = Comment + jsonClass.interactiondata.fullInteractionTypeGroup.fullInteractionType.comment;
                Pair = jsonClass.interactiondata.fullInteractionTypeGroup.fullInteractionType.interactionPair;
       
                foreach (var pair in Pair)
                {
                    Comment += pair.description;
                    Severity.Add(pair.severity);
                }

            }
            catch (Exception)
            {
                return new List<string>() { "", "false" };
            }

            List<string> strings = new List<string>();
            strings.Add(Comment);

            bool flag = false;
            for (int i = 0; i < Severity.Count(); i++)
            {
                if (Severity[i] == "high" || Severity[i] == "N/A")
                    flag = true; //there's dangerous in the interaction between them
            }
            strings.Add(flag.ToString());

            return strings;
        }
        public bool IsFalse(string s)
        {
            if (s == "false")
                return true;
            return false;
        }
        #endregion

        public void Dispose()
        {
            DrugsContext db = new DrugsContext();
            db.Dispose();
        }
    }
}
