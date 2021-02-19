using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Hospital_Management_System.CollectionViewModels;
using Hospital_Management_System.Models;

namespace Hospital_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db;

        private ApplicationUserManager _userManager;

        //Constructor
        public AdminController()
        {
            db = new ApplicationDbContext();
        }

        //Destructor
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string message)
        {
            var date = DateTime.Now.Date;
            ViewBag.Messege = message;
            var model = new CollectionOfAll
            {
                Departments = db.Department.ToList(),
                Doctors = db.Doctors.ToList(),
                Patients = db.Patients.ToList(),
                ActiveAppointments =
                    db.Appointments.Where(c => c.Status).Where(c => c.AppointmentDate >= date).ToList(),
                PendingAppointments = db.Appointments.Where(c => c.Status == false)
                    .Where(c => c.AppointmentDate >= date).ToList(),
              
            };
            return View(model);
        }

        //Department Section

        //Department List
        [Authorize(Roles = "Admin")]
        public ActionResult DepartmentList()
        {
            var model = db.Department.ToList();
            return View(model);
        }

        //Add Department
        [Authorize(Roles = "Admin")]
        public ActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartment(Department model)
        {
            if (db.Department.Any(c => c.Name == model.Name))
            {
                ModelState.AddModelError("Name", "Name already present!");
                return View(model);
            }

            db.Department.Add(model);
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        //Edit Department
        [Authorize(Roles = "Admin")]
        public ActionResult EditDepartment(int id)
        {
            var model = db.Department.SingleOrDefault(c => c.Id == id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDepartment(int id, Department model)
        {
            var department = db.Department.Single(c => c.Id == id);
            department.Name = model.Name;
            department.Description = model.Description;
            department.Status = model.Status;
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDepartment(int? id)
        {
            var department = db.Department.Single(c => c.Id == id);
            return View(department);
        }

        [HttpPost, ActionName("DeleteDepartment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDepartment(int id)
        {
            var department = db.Department.SingleOrDefault(c => c.Id == id);
            db.Department.Remove(department);
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        //End Department Section

       

        //Start Doctor Section

        //Add Doctor 
        [Authorize(Roles = "Admin")]
        public ActionResult AddDoctor()
        {
            var collection = new DoctorCollection
            {
                ApplicationUser = new RegisterViewModel(),
                Doctor = new Doctor(),
                Departments = db.Department.ToList()
            };
            return View(collection);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDoctor(DoctorCollection model)
        {
            var user = new ApplicationUser
            {
                UserName = model.ApplicationUser.UserName,
                Email = model.ApplicationUser.Email,
                UserRole = "Doctor",
                RegisteredDate = DateTime.Now.Date
            };
            var result = await UserManager.CreateAsync(user, model.ApplicationUser.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "Doctor");
                var doctor = new Doctor
                {
                    FirstName = model.Doctor.FirstName,
                    LastName = model.Doctor.LastName,
                    FullName = "Dr. " + model.Doctor.FirstName + " " + model.Doctor.LastName,
                    EmailAddress = model.ApplicationUser.Email,
                    ContactNo = model.Doctor.ContactNo,
                    PhoneNo = model.Doctor.PhoneNo,
                    Designation = model.Doctor.Designation,
                    Education = model.Doctor.Education,
                    DepartmentId = 1,
                    Specialization = model.Doctor.Specialization,
                    Gender = model.Doctor.Gender,
                    BloodGroup = model.Doctor.BloodGroup,
                    ApplicationUserId = user.Id,
                    DateOfBirth = model.Doctor.DateOfBirth,
                    Address = model.Doctor.Address,
                    Status = model.Doctor.Status
                };
                db.Doctors.Add(doctor);
                db.SaveChanges();
                return RedirectToAction("ListOfDoctors");
            }

            return View();

        }


        //List Of Doctors
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfDoctors()
        {
            var doctor = db.Doctors.Include(c => c.Department).ToList();
            return View(doctor);
        }

        //Detail of Doctor
        [Authorize(Roles = "Admin")]
        public ActionResult DoctorDetail(int id)
        {
            var doctor = db.Doctors.Include(c => c.Department).SingleOrDefault(c => c.Id == id);
            return View(doctor);
        }

        //Edit Doctors
        [Authorize(Roles = "Admin")]
        public ActionResult EditDoctors(int id)
        {
            var collection = new DoctorCollection
            {
                Departments = db.Department.ToList(),
                Doctor = db.Doctors.Single(c => c.Id == id)
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDoctors(int id, DoctorCollection model)
        {
            var doctor = db.Doctors.Single(c => c.Id == id);
            doctor.FirstName = model.Doctor.FirstName;
            doctor.LastName = model.Doctor.LastName;
            doctor.FullName = "Dr. " + model.Doctor.FirstName + " " + model.Doctor.LastName;
            doctor.ContactNo = model.Doctor.ContactNo;
            doctor.PhoneNo = model.Doctor.PhoneNo;
            doctor.Designation = model.Doctor.Designation;
            doctor.Education = model.Doctor.Education;
            doctor.DepartmentId = model.Doctor.DepartmentId;
            doctor.Specialization = model.Doctor.Specialization;
            doctor.Gender = model.Doctor.Gender;
            doctor.BloodGroup = model.Doctor.BloodGroup;
            doctor.DateOfBirth = model.Doctor.DateOfBirth;
            doctor.Address = model.Doctor.Address;
            doctor.Status = model.Doctor.Status;
            db.SaveChanges();

            return RedirectToAction("ListOfDoctors");
        }

        //Delete Doctor
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDoctor(string id)
        {
            var UserId = db.Doctors.Single(c => c.ApplicationUserId == id);
            return View(UserId);
        }

        [HttpPost, ActionName("DeleteDoctor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDoctor(string id, Doctor model)
        {
            var doctor = db.Doctors.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
           

            db.Users.Remove(user);
            db.Doctors.Remove(doctor);
            db.SaveChanges();
            return RedirectToAction("ListOfDoctors");
        }

        //End Doctor Section

       

        //Start Patient Section

        //List of Patients
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfPatients()
        {
            var patients = db.Patients.ToList();
            return View(patients);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditPatient(int id)
        {
            var patient = db.Patients.Single(c => c.Id == id);
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPatient(int id, Patient model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var patient = db.Patients.Single(c => c.Id == id);
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.FullName = model.FirstName + " " + model.LastName;
            patient.Address = model.Address;
            patient.BloodGroup = model.BloodGroup;
            patient.Contact = model.Contact;
            patient.DateOfBirth = model.DateOfBirth;
            patient.EmailAddress = model.EmailAddress;
            patient.Gender = model.Gender;
            patient.PhoneNo = model.PhoneNo;
            db.SaveChanges();
            return RedirectToAction("ListOfPatients");
        }

        //Delete Patient
        [Authorize(Roles = "Admin")]
        public ActionResult DeletePatient()
        {
            return View();
        }

        [HttpPost, ActionName("DeletePatient")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePatient(string id)
        {
            var patient = db.Patients.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
            db.Users.Remove(user);
            db.Patients.Remove(patient);
            db.SaveChanges();
            return RedirectToAction("ListOfPatients");
        }

        //End Patient Section

        //Start Appointment Section

        //Add Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult AddAppointment()
        {
            var collection = new AppointmentCollection
            {
                Appointment = new Appointment(),
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAppointment(AppointmentCollection model)
        {
            var collection = new AppointmentCollection
            {
                Appointment = model.Appointment,
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            if (model.Appointment.AppointmentDate >= DateTime.Now.Date)
            {
                var appointment = new Appointment();
                appointment.PatientId = model.Appointment.PatientId;
                appointment.DoctorId = model.Appointment.DoctorId;
                appointment.AppointmentDate = model.Appointment.AppointmentDate;
                appointment.Problem = model.Appointment.Problem;
                appointment.Status = model.Appointment.Status;
                db.Appointments.Add(appointment);
                db.SaveChanges();

                if (model.Appointment.Status == true)
                {
                    return RedirectToAction("ListOfAppointments");
                }
                else
                {
                    return RedirectToAction("PendingAppointments");
                }
            }

            ViewBag.Messege = "Please Enter the Date greater than today or equal!!";
            return View(collection);

        }

        //List of Active Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient)
                .Where(c => c.Status == true).Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }

        //List of pending Appointments
        [Authorize(Roles = "Admin")]
        public ActionResult PendingAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient)
                .Where(c => c.Status == false).Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }

        //Edit Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult EditAppointment(int id)
        {
            var collection = new AppointmentCollection
            {
                Appointment = db.Appointments.Single(c => c.Id == id),
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAppointment(int id, AppointmentCollection model)
        {
            var collection = new AppointmentCollection
            {
                Appointment = model.Appointment,
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            if (model.Appointment.AppointmentDate >= DateTime.Now.Date)
            {
                var appointment = db.Appointments.Single(c => c.Id == id);
                appointment.PatientId = model.Appointment.PatientId;
                appointment.DoctorId = model.Appointment.DoctorId;
                appointment.AppointmentDate = model.Appointment.AppointmentDate;
                appointment.Problem = model.Appointment.Problem;
                appointment.Status = model.Appointment.Status;
                db.SaveChanges();
                if (model.Appointment.Status == true)
                {
                    return RedirectToAction("ListOfAppointments");
                }
                else
                {
                    return RedirectToAction("PendingAppointments");
                }
            }
            ViewBag.Messege = "Please Enter the Date greater than today or equal!!";

            return View(collection);
        }

        //Detail of Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult DetailOfAppointment(int id)
        {
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient).Single(c => c.Id == id);
            return View(appointment);
        }

        //Delete Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAppointment(int? id)
        {
            var appointment = db.Appointments.Single(c => c.Id == id);
            return View(appointment);
        }

        [HttpPost, ActionName("DeleteAppointment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAppointment(int id)
        {
            var appointment = db.Appointments.Single(c => c.Id == id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            if (appointment.Status)
            {
                return RedirectToAction("ListOfAppointments");
            }
            else
            {
                return RedirectToAction("PendingAppointments");
            }
        }

        //End Appointment Section



       
    }
}