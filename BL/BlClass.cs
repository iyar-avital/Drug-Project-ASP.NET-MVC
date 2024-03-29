﻿using BE;
using DAL;
using System;
using System.CodeDom;
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
        public void AddDoctor(Doctor doctor)
        {
            try
            {
                IDAL dal = new DalClass();
                IEnumerable<Doctor> doctors = dal.GetDoctors(doc => doc.idNumber == doctor.idNumber);
                if (doctors.Count() != 0)
                    throw new Exception("רופא זה כבר רשום במערכת");
                dal.AddDoctor(doctor);
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
                IDAL dal = new DalClass();
                medicine.NDC = GetNDCForMedicine(medicine.genericaName);
                bool IsOkImage = ValidateImage(medicine.imagePath);

                if (IsOkImage)
                {
                    medicine.manufacturer = medicine.manufacturer == null ? "לא ידוע" : medicine.manufacturer;
                    dal.AddMedicine(medicine, httpPostedFile);
                }
                else
                    throw new Exception("תמונה זו אינה מתארת תרופה. אנא נסה שוב עם תוכן מתאים יותר." 
                         + " המלצתנו היא שתבחר תמונה אחרת לתרופה שברצונך להוסיף למערכת.");
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
                IDAL dal = new DalClass();
                IEnumerable<Patient> patients = dal.GetPatients(p => p.idNumber == patient.idNumber);
                if (patients.Count() != 0)
                    throw new Exception("מטופל זה כבר רשום במערכת");
                patient.medicalHistory = patient.medicalHistory == null ? "לא נמסר מידע" : patient.medicalHistory;
                dal.AddPatient(patient);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void AddPrescription(Prescription prescription)
        {
            IDAL dal = new DalClass();
            if (prescription.startDate < DateTime.Now)
                throw new Exception("תאריך תחילת המרשם חייב להיות החל מהיום");
            if (prescription.endDate < prescription.startDate)
                throw new Exception("תאריך תחילת המרשם חייב להיות טרם סיומו");

            //Obtaining a NDC number of all active prescriptions for this patient
            List<string> NDCforPatientMedicines = GetNDCForAllActiveMedicine(prescription.PatientId);

            Medicine med = GetMedicine(prescription.MedicineId);
            NDCforPatientMedicines.Add(med.NDC);

            List<string> Result = IsConflict(NDCforPatientMedicines);
            bool isConflict = Result[1] == "True" ? true : false;
            if (isConflict)
                throw new Exception("נמצא ניגוד מרכיבים בין התרופות שהמטופל נוטל לבין זו החדשה. מחובתנו לדאוג לבריאות המטופל ולכן לא נוסיף לו מרשם זה. פירוט הודעת השגיאה: "+"\n" + Result[0]);
            else
                dal.AddPrescription(prescription);
        }

        #endregion

        #region UPDATE
        public void UpdateDoctor(Doctor doctor)
        {
            try
            {
                IDAL dal = new DalClass();
                IEnumerable<Doctor> doctors = dal.GetDoctors(doc => doc.idNumber == doctor.idNumber);
                if (doctors.Count() == 0)
                    throw new Exception("רופא זה לא מוכר במערכת, אנא הוסף אותו");
                dal.UpdateDoctor(doctor);
            }
            catch (Exception ex)
            {
                throw ex;
            }          
        }

        public void UpdateMedicine(Medicine medicine, HttpPostedFileBase httpPostedFile)
        {
            try
            {
                IDAL dal = new DalClass();
                bool IsOkImage = ValidateImage(medicine.imagePath);

                if (IsOkImage)
                {
                    medicine.manufacturer = medicine.manufacturer == null ? "לא ידוע" : medicine.manufacturer;
                    dal.UpdateMedicine(medicine, httpPostedFile);
                }
                else
                   throw new Exception("תמונה זו אינה מתארת תרופה. אנא נסה שוב עם תוכן מתאים יותר." + "\n"
                             + " המלצתנו היא שתבחר תמונה אחרת לתרופה שברצונך להוסיף למערכת.");
                
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
                IDAL dal = new DalClass();
                IEnumerable<Patient> patients = dal.GetPatients(p => p.idNumber == patient.idNumber);
                if (patients.Count() == 0)
                    throw new Exception("מטופל זה לא מוכר במערכת, אנא הוסף אותו");
                patient.medicalHistory = patient.medicalHistory == null ? "לא נמסר מידע" : patient.medicalHistory;
                dal.UpdatePatient(patient);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        public IEnumerable<MedicineWrraper> GetAllNDC()
        {
            IDAL dal = new DalClass();
            return dal.GetAllNDC();
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
        public string GetNDCForMedicine(string genericName)
        {
            IDAL dal = new DalClass();
            return dal.GetNDCForMedicine(genericName);
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

        #endregion

        #region VALIDATION
        public Dictionary<string, string> PersonValidation(Person person)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (!Validation.IsId(person.idNumber))
                result.Add("idNumber", "מספר תעודת הזהות אינו תקין");
            if (!Validation.IsEmail(person.email))
                result.Add("email", "כתובת המייל אינה תקינה");
            if (!Validation.IsPhone(person.phoneNumber))
                result.Add("phoneNumber", "מספר הטלפון אינו תקין");
            if (!Validation.IsName(person.familyName))
                result.Add("familyName", "שם זה אינו תקין");
            if (!Validation.IsName(person.privateName))
                result.Add("privateName", "שם זה אינו תקין");
            return result;
        }

        public Dictionary<string, string> SignValidation(DoctorSign doctorSign)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (doctorSign.idNumber == null)
                result.Add("idNumber", "חובה להזין מספר ת.ז.");
            else if (!Validation.IsId(doctorSign.idNumber))
                result.Add("idNumber", "מספר תעודת הזהות אינו תקין");
            if (doctorSign.email == null)
                result.Add("email", "חובה להזין כתובת מייל");
            else if (!Validation.IsEmail(doctorSign.email))
                result.Add("email", "כתובת המייל אינה תקינה");
            if (doctorSign.password == null)
                result.Add("password", "חובה לבחור סיסמה");
            else if (!Validation.IsPassword(doctorSign.password))
                result.Add("password", "סיסמה אינה תקינה");
            return result;
        }

        #endregion

        #region ACCOUNT
        public void SignIn(DoctorSign doctorSign)
        {
            try
            {
                IDAL dal = new DalClass();
                Doctor doctor = dal.GetDoctors(doc => doc.email == doctorSign.email).FirstOrDefault();
                if (doctor == null && doctorSign.email != "MyProject4Ever@gmail.com")
                    throw new Exception("כתובת המייל לא זוהתה במערכת. אנא בדוק אותה או בצע הרשמה");
                else if (doctor == null || doctorSign.password != doctor.password)
                    throw new Exception("סיסמה שגויה, נסה שנית");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SignUp(DoctorSign doctorSign)
        {
            IDAL dal = new DalClass();
            Doctor doctor= dal.GetDoctors(doc => doc.idNumber == doctorSign.idNumber).FirstOrDefault();
            if (doctor != null && doctor.password == null)
            {
                doctor.password = doctorSign.password;
                dal.UpdateDoctor(doctor);
                SendMail(doctor.email, "ההרשמה עברה בהצלחה", doctor.privateName + " " + doctor.familyName, "ברוכים הבאים לאתר שלנו, שמחים שהצטרפת." + "<br/>"
                         + "נשמח לעמוד לעזרתך בכל פניה ובקשה ומקווים שתהיה לך חוויה נעימה." + "<br/>" + "תודה, צוות אח לאח");
            }
            else if(doctor == null)
            {
                SendMail(doctorSign.email, "ההרשמה נכשלה", "", "לצערנו, נסיון ההרשמה שלך לאתרנו נכשל." + "<br/>"
                         + "אנא נסה שוב בעוד חצי שנה ונשמח לעמוד לעזרתך." + "<br/>" + "תודה, צוות אח לאח");
                throw new Exception("ניסיון הרשמתך לאתרנו נכשל, אנא בדוק את תיבת המייל שלך עבור פרטים נוספים.");
            }
            else
            {
                throw new Exception("הנך רשום למערכת, אנא בצע התחברות. אם שכחת סיסמא לחץ על 'שכחת סיסמה?'");
            }
        }
        public void ForgotPassword(string mail)
        {
            try
            {
                IDAL dal = new DalClass();
                Doctor doctor = GetDoctors(doc => doc.email == mail).FirstOrDefault();
                if (doctor != null)
                {
                    Random random = new Random();
                    int newPassword = random.Next(10000, 1000000000);
                    SendMail(doctor.email, "איפוס סיסמה", doctor.privateName + " " + doctor.familyName, "הסיסמה שלך אופסה, הסיסמה החדשה היא:" + "<br/>" + newPassword + "<br/>" + "אנא שנה סיסמה בהקדם האפשרי");

                    //Edit Password
                    doctor.password = newPassword.ToString();
                    UpdateDoctor(doctor);
                }
                else
                    throw new Exception("המייל שהוזן שגוי. בדוק אם הינך רשום למערכת (איך?! תפעיל את הזיכרון בבקשה.)");
            }
            catch (Exception ex)
            {
                throw ex;
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
            mail.From = new MailAddress("MyProject4Ever@gmail.com");
            mail.Subject = subject;
            mail.Body =
               "<b> <p style = 'font-size:25px'>" + "שלום " + receiverName + "</p> </b>" +
               "<b> <p style = 'font-size:20px'>" + message + "</p> </b>";
            mail.IsBodyHtml = true;
            smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Credentials = new System.Net.NetworkCredential("MyProject4Ever@gmail.com", "bla/*123*/bla");
            smtp.EnableSsl = true;
            smtp.Send(mail);
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
