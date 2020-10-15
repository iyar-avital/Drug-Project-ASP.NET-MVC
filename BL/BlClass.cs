﻿using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Policy;
using System.Web;

namespace BL
{
    public class BlClass : IBL
    {
        #region ADD
        public bool AddCronicalDisease(CronicalDisease cronicalDisease)
        {
            IDAL dal = new DalClass();
            return dal.AddCronicalDisease(cronicalDisease);
        }

        public bool AddDoctor(Doctor doctor)
        {
            IDAL dal = new DalClass();
            return dal.AddDoctor(doctor);
        }

        public bool AddMedicine(Medicine medicine)
        {
            IDAL dal = new DalClass();
            bool IsOkImage = ValidateImage(medicine.imagePath);
            if (IsOkImage)
                return dal.AddMedicine(medicine);
            else
                return false;
        }

        public bool AddPatient(Patient patient)
        {
            IDAL dal = new DalClass();
            return dal.AddPatient(patient);
        }

        public bool AddPrescription(Prescription prescription)
        {
            IDAL dal = new DalClass();
            List<string> NDCforPatientMedicines = GetNDCForAllActiveMedicine(prescription.PatientId);
            List<string> Result = IsConflict(NDCforPatientMedicines);
            bool isConflict = Result[1] == "true" ? true : false;
            if (isConflict)
                return false;
            else
                return dal.AddPrescription(prescription);
        }
        #endregion

        #region UPDATE
        public bool UpdateDoctor(Doctor doctor)
        {
            IDAL dal = new DalClass();
            return dal.UpdateDoctor(doctor);
        }

        public bool UpdateMedicine(Medicine medicine)
        {
            IDAL dal = new DalClass();
            return dal.UpdateMedicine(medicine);
        }

        public bool UpdatePatient(Patient patient)
        {
            IDAL dal = new DalClass();
            return dal.UpdatePatient(patient);
        }
        #endregion

        #region DELETE
        public bool DeleteDoctor(int? id)
        {
            IDAL dal = new DalClass();
            return dal.DeleteDoctor(id);
        }

        public bool DeleteMedicine(int? id)
        {
            IDAL dal = new DalClass();
            return dal.DeleteMedicine(id);
        }

        public bool DeletePatient(int? id)
        {
            IDAL dal = new DalClass();
            return dal.DeletePatient(id);
        }
        #endregion

        #region GET LISTS     
        public IEnumerable<Person> GetAllPerson(Func<Person, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            IEnumerable<Person> doctors = dal.GetDoctors();
            IEnumerable<Person> patients = dal.GetPatients();

            IEnumerable<Person> all = doctors.Union(patients);
            return all;
        }

        public IEnumerable<Doctor> GetDoctors(Func<Doctor, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            return dal.GetDoctors(predicat);
        }
        public IEnumerable<Medicine> GetMedicines(Func<Medicine, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            return dal.GetMedicines(predicat);
        }

        public List<string> GetNDCForAllActiveMedicine(int id)
        {
            IEnumerable<Medicine> medicinePatient = FilterActiveMedicinesForPatient(id);
            List<string> NDC = medicinePatient.Select(med => med.NDC).ToList();
            return NDC;
        }

        public IEnumerable<Patient> GetPatients(Func<Patient, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            return dal.GetPatients(predicat);
        }
        public IEnumerable<Prescription> GetPrescriptions(Func<Prescription, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            return dal.GetPrescriptions(predicat);
        }
        public IEnumerable<CronicalDisease> GetCronicalDiseases(Func<CronicalDisease, bool> predicat = null)
        {
            IDAL dal = new DalClass();
            return dal.GetCronicalDiseases(predicat);
        }

        #endregion

        #region GET
        public Doctor GetDoctor(int? id)
        {
            IDAL dal = new DalClass();
            return dal.GetDoctor(id);
        }

        public Medicine GetMedicine(int? id)
        {
            IDAL dal = new DalClass();
            return dal.GetMedicine(id);
        }

        public Patient GetPatient(int? id)
        {
            IDAL dal = new DalClass();
            return dal.GetPatient(id);
        }

        public Prescription GetPrescription(int? id)
        {
            IDAL dal = new DalClass();
            return dal.GetPrescription(id);
        }
        #endregion

        #region FILTER
        public IEnumerable<Prescription> FilterPrescriptionsForPatient(int patientID)
        {
            IDAL dal = new DalClass();
            return dal.GetPrescriptions(pre => pre.PatientId == patientID);
        }

        public IEnumerable<Prescription> FilterActivePrescriptionsForPatient(int patientID)
        {
            IDAL dal = new DalClass();
            return dal.GetPrescriptions(pre => pre.PatientId == patientID && pre.endDate >= DateTime.Now);
        }
        public IEnumerable<Medicine> FilterActiveMedicinesForPatient(int patientID)
        {
            IEnumerable<Prescription> prescriptions = FilterActivePrescriptionsForPatient(patientID);
            var medicinesId = prescriptions.Select(pre => pre.MedicineId);
            IEnumerable<Medicine> medicines = medicinesId.Select(medId => GetMedicine(medId));

            return medicines;
        }

        public IEnumerable<CronicalDisease> FilterCronicalDiseasesForPatient(int patientID)
        {
            IDAL dal = new DalClass();
            return dal.GetCronicalDiseases(pre => pre.PatientId == patientID);
        }
        #endregion

        #region ACCOUNT
        public bool SignIn(string userName, string password)
        {
            IDAL dal = new DalClass();
            Doctor doctor = dal.GetDoctors(doc => doc.userName == userName).FirstOrDefault();
            if (doctor != null && doctor.password == password)
                return true;
            return false;
        }
        public void SignUp(Person person)
        {
            throw new NotImplementedException();
        }
        public void ForgotPassword(string mail)
        {
            IDAL dal = new DalClass();
            //Person person = GetAllPerson(per=> per.email == mail).FirstOrDefault();
            Doctor doctor = GetDoctors(doc => doc.email == mail).FirstOrDefault();
            if (doctor != null)
            {
                Random random = new Random();
                int newPassword = random.Next(10000, 1000000000);
                SendMail(doctor.email, doctor.privateName + " " + doctor.familyName, "איפוס סיסמה", "הסיסמה שלך אופסה, הסיסמה החדשה היא:" + "<br/>" + newPassword + "<br/>" + "אנא שנה סיסמה בהקדם האפשרי");
               
                //Edit(user.Id, Password);
                doctor.password = newPassword.ToString();
                UpdateDoctor(doctor);
            }
        }
        #endregion

        #region SEND
        public void SendMail(string mailAdress, string subject, string receiverName, string message)
        {
            MailMessage mail;
            SmtpClient smtp;
            mail = new MailMessage();
            mail.To.Add(mailAdress);
            mail.From = new MailAddress("deamlandapp@gmail.com");  //openMail
            mail.Subject = subject;
            mail.Body = $"שלום {receiverName}, <br>" + message;
            mail.IsBodyHtml = true;
            smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Credentials = new System.Net.NetworkCredential("deamlandapp@gmail.com", "0533151327");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        #endregion

        #region CODE
        public int GetRandomCode()
        {
            Random random = new Random();
            return random.Next(10000, 99999); //5 digits code
        }
        public bool CheckOneTimeCode(int randonCode, int codeEntered)
        {
            return randonCode == codeEntered;
        }
        #endregion

        #region IMAGE SERVICE
        public bool ValidateImage(string _path)
        {
            List<string> testImages = new List<string>()
            {"pill", "pill bottle", "pills", "medicine", "bottle", "syrup", "medical", "drug", "drugs", "cure", "prescription drug", "vitamin", "cream", "ointment"};

            List<string> Result = new List<string>(); //returning the list of results order by certain confidence
            bool flag = false;

            string path = ConvertStringToUrl(_path);

            ImageDetails DrugImage = new ImageDetails(path);

            IDAL dal = new DalClass();

            dal.GetTags(DrugImage);

            var Threshold = 60.0; //testing the result with confidence above 60%

            foreach (var item in DrugImage.Details)
            {
                if (item.Value > Threshold)
                {
                    foreach (var option in testImages) //the words we can accept
                    {
                        if (item.Key == option)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }

            return flag; /*Result;*/
        }
        public string ConvertStringToUrl(string path)
        {
            string imgPath = HttpContext.Current.Server.MapPath($"~/img/{path}");
            return imgPath;
        }
        #endregion

        #region CONFLICT DRUGS SERVICE 
        public List<string> IsConflict(List<string> NDC)
        {
            IDAL dal = new DalClass();

            List<string> strings = dal.IsConflict(NDC);
            return strings;
        }
        #endregion

        public void Dispose(bool disp)
        {
            if (disp)
            {
                IDAL dal = new DalClass();
                dal.Dispose();
            }
        }

    }
}