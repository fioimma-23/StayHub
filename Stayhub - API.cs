using DataAccessLayer.Models;
using DataAccessLayer.Models.APIModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Twilio.Types;
//using MailKit.Net.Smtp;
//using MailKit;
//using MimeKit;
//using System.Net.Mail;

namespace DataAccessLayer.DataMethods
{
    public class RoomBookingManagement : DbEntity
    {
        static string key = "b14ca5898a4e4133bbce2ea2315a1916";
        //private string? phoneNumber;

        public string Password { get; private set; }

        public Response<Object> validateUserLogin(String UserId, string Password)
        {
            var response = new Response<Object>();

            try
            {


                Password = EncryptString(key, Password);

                var userLogin = (
                   from Users in context.users
                   where Users.UserId == UserId && Users.Password == Password && Users.IsActive == true
                   select new UserResponse
                   {

                       UserId = Users.UserId,
                       UserName = Users.UserName,
                       IsActive = Users.IsActive,
                       Role = Users.Role,
                       email = Users.email,
                       Rollno = Users.Rollno,
                       SYear = Users.SYear,
                       Dept = Users.Dept,
                       Semester = Users.Semester
                   }
               ).FirstOrDefault();

                if (userLogin != null)
                {
                    response.Result = userLogin;
                    response.ErrorCode = ResponseStatus.Success;
                    response.ErrorMessage = ResponseMessages.Success;
                }
                else
                {
                    response.ErrorCode = ResponseStatus.InvalidUser;
                    response.ErrorMessage = ResponseMessages.InvalidUser;
                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;

        }



        public Response<UsersUpdate> Registration(UsersInputs userData)
        {
            var response = new Response<UsersUpdate>();

            try
            {
                if (userData != null)
                {

                    if (userData.Action == "A")
                    {
                        var newUser = new UsersUpdate();
                        newUser.UserName = userData.UserName;
                        newUser.Password = EncryptString(key, userData.Password);
                        newUser.UserId = userData.UserId;
                        newUser.IsActive = true;
                        newUser.Role = userData.Role;
                        newUser.email = userData.email;
                        newUser.Rollno = userData.Rollno;
                        newUser.Dept = userData.Dept;
                        newUser.SYear = userData.SYear;
                        newUser.Semester = userData.Semester;

                        context.users.Add(newUser);
                        context.SaveChanges();
                    }
                    else
                    {

                        var existingUser = context.users.Where(x => x.UserId == userData.UserId).FirstOrDefault();

                        if (existingUser != null)
                        {
                            using (var scope = new TransactionScope(TransactionScopeOption.Required))
                            {

                                existingUser.UserName = userData.UserName;
                                existingUser.Password = EncryptString(key, userData.Password);
                                //existingUser.About = userData.About;
                                existingUser.UserId = userData.UserId;
                                existingUser.IsActive = true;

                                context.SaveChanges();
                                scope.Complete();

                                response.ErrorCode = ResponseStatus.Success;
                                response.ErrorMessage = ResponseMessages.Success;
                            }

                        }

                        response.ErrorCode = ResponseStatus.Success;
                        response.ErrorMessage = ResponseMessages.Success;

                    }
                }


                else
                {

                    response.ErrorCode = ResponseStatus.Failure;
                    response.ErrorMessage = ResponseMessages.NoData;
                }
            }

            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;
        }


        public Response<List<UsersUpdate>> updatePassword(string UserId, string Password)
        {
            var response = new Response<List<UsersUpdate>>();


            try
            {

                var existingUser = context.users.Where(x => x.UserId == UserId).FirstOrDefault();

                if (existingUser != null)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {

                        existingUser.Password = EncryptString(key, Password);


                        context.SaveChanges();
                        scope.Complete();

                        response.ErrorCode = ResponseStatus.Success;
                        response.ErrorMessage = ResponseMessages.Success;
                    }

                }

                else
                {
                    response.ErrorCode = ResponseStatus.Failure;
                    response.ErrorMessage = ResponseMessages.ServerError;

                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;

        }

        public Response<Object> GetGHAvailable(string GHName, String GHCategory, DateTime FromDate, DateTime toDate)
        {
            var response = new Response<Object>();

            try
            {

                var GHAvilability = (
                   from GHAvilable in context.GHMasterAvilable
                   select new GHMasterAvilablityResponse
                   {

                       RowId = GHAvilable.RowId,
                       GHId = GHAvilable.GHId,
                       GHName = GHAvilable.GHName,
                       GHCategory = GHAvilable.GHCategory,
                       GHNo = GHAvilable.GHNo,
                       DateFrom = GHAvilable.DateFrom,
                       DateTo = GHAvilable.DateTo,

                   }
               ).ToList();

                if (GHAvilability != null)
                {
                    response.Result = GHAvilability;
                    response.ErrorCode = ResponseStatus.Success;
                    response.ErrorMessage = ResponseMessages.Success;
                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;

        }

        //public Response<Object> GHBooking(PostData GHBKData)
        //{
        //    int RoomCnt = 0 ;
        //    var response = new Response<Object>();
        //    var GKBKheader = GHBKData.GHBKHeader;
        //    var GKBKdetails = GHBKData.GhbkDetail;

        //    try
        //        {
        //        if ((GKBKheader != null) && (GKBKdetails != null))
        //        {
        //            string NewBookingId = GetLastBookingId();

        //            var newGHBookMas = new RoomBookMasUpdate();
        //            newGHBookMas.BookingId = NewBookingId;
        //            newGHBookMas.BookName = GKBKheader.BookName;
        //            newGHBookMas.Rollno = GKBKheader.Rollno;
        //            newGHBookMas.RoomCount = GKBKheader.RoomCount;
        //            newGHBookMas.DateFrom = GKBKheader.DateFrom;
        //            newGHBookMas.DateTo = GKBKheader.DateTo;
        //            newGHBookMas.Status = "O";
        //            newGHBookMas.Approved = "N";

        //            context.RoomBookMasUpd.Add(newGHBookMas);
        //            context.SaveChanges();


        //            var newGHBookTran = new RoomBookTranUpdate();

        //            foreach (var GKBKdetail in GKBKdetails)
        //            {

        //            newGHBookTran.BookingId = NewBookingId;
        //            newGHBookTran.RoomId = GKBKdetail.RoomId;
        //            newGHBookTran.RoomCount = ++RoomCnt;
        //            newGHBookTran.Status = "O";

        //            context.RoomBookTranUpd.Add(newGHBookTran);
        //            context.SaveChanges();

        //            }
        //            response.ErrorCode = ResponseStatus.Success;
        //            response.ErrorMessage = ResponseMessages.Success;
        //        }
        //        else
        //        {
        //            response.ErrorCode = ResponseStatus.Error;
        //            response.ErrorMessage = "Blank Data";
        //        }
        //    }

        //        catch (Exception e)
        //        {
        //            response.ErrorCode = ResponseStatus.Error;
        //            response.ErrorMessage = e.ToString();
        //        }
        //        return response;
        //    }

        public Response<Object> GHBookingRequest(GHBookingMasInputs GHBKData)
        {

            var response = new Response<Object>();

            try
            {
                if (GHBKData != null)
                {
                    string NewBookingId = GetLastBookingId();

                    var newGHBookMas = new GHBookingMasUpdate();

                    newGHBookMas.BookingId = NewBookingId;
                    newGHBookMas.GuestName = GHBKData.GuestName;
                    newGHBookMas.GuestContactNo = GHBKData.GuestContactNo;
                    newGHBookMas.GuestEmail = GHBKData.GuestEmail;
                    newGHBookMas.DateFrom = GHBKData.DateFrom;
                    newGHBookMas.DateTo = GHBKData.DateTo;
                    newGHBookMas.NoofRooms = GHBKData.NoofRooms;
                    newGHBookMas.NoofPersons = GHBKData.NoofPersons;
                    newGHBookMas.Category = GHBKData.Category;
                    newGHBookMas.BookingName = GHBKData.BookingName;
                    newGHBookMas.StudOrStaffID = GHBKData.StudOrStaffID;
                    newGHBookMas.Designation = GHBKData.Designation;
                    newGHBookMas.BookingContactNo = GHBKData.BookingContactNo;
                    newGHBookMas.Department = GHBKData.Department;
                    newGHBookMas.BookingEmail = GHBKData.BookingEmail;
                    newGHBookMas.Food = GHBKData.Food;
                    newGHBookMas.Status = "O";
                    newGHBookMas.Approved = "N";

                    context.GHBookingMasUpd.Add(newGHBookMas);
                    context.SaveChanges();



                }
                else
                {
                    response.ErrorCode = ResponseStatus.Error;
                    response.ErrorMessage = "Blank Data";
                }
            }

            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;
        }
        public Response<Object> GetBookingId()
        {
            var response = new Response<Object>(); ;

            try
            {
                var GHAvilability = (
                   from GHAvilable in context.GHMasterAvilable
                   select new GHMasterAvilablityResponse
                   {

                       RowId = GHAvilable.RowId,
                       GHId = GHAvilable.GHId,
                       GHName = GHAvilable.GHName,
                       GHCategory = GHAvilable.GHCategory,
                       GHNo = GHAvilable.GHNo,
                       DateFrom = GHAvilable.DateFrom,
                       DateTo = GHAvilable.DateTo,

                   }
               ).ToList();

                if (GHAvilability != null)
                {
                    response.Result = GHAvilability;
                    response.ErrorCode = ResponseStatus.Success;
                    response.ErrorMessage = ResponseMessages.Success;
                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;
        }


        public Response<Object> GetGHBookingInfo(string BookingId)
        {
            var response = new Response<Object>();

            try
            {

                var GHBookingInfo = (
                   from GHBkInfor in context.GHBookingMasUpd
                   where GHBkInfor.BookingId == BookingId
                   select new GHBookingMasInputs
                   {

                       RowId = GHBkInfor.RowId,
                       BookingId = GHBkInfor.BookingId,
                       GuestName = GHBkInfor.GuestName,
                       GuestContactNo = GHBkInfor.GuestContactNo,
                       GuestEmail = GHBkInfor.GuestEmail,
                       DateFrom = GHBkInfor.DateFrom,
                       DateTo = GHBkInfor.DateTo,
                       NoofRooms = GHBkInfor.NoofRooms,
                       NoofPersons = GHBkInfor.NoofPersons,
                       Category = GHBkInfor.Category,
                       BookingName = GHBkInfor.BookingName,
                       StudOrStaffID = GHBkInfor.StudOrStaffID,
                       Designation = GHBkInfor.Designation,
                       BookingContactNo = GHBkInfor.BookingContactNo,
                       Department = GHBkInfor.Department,
                       BookingEmail = GHBkInfor.BookingEmail,
                       Food = GHBkInfor.Food,
                       Status = GHBkInfor.Status,
                       Approved = GHBkInfor.Approved,

                   }
               ).ToList();

                if (GHBookingInfo != null)
                {
                    response.Result = GHBookingInfo;
                    response.ErrorCode = ResponseStatus.Success;
                    response.ErrorMessage = ResponseMessages.Success;
                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;

        }

        public Response<object> GHBookingCancel(string BookingID)
        {
            var response = new Response<object>();

            try
            {
                var FetchBookingId = context.GHBookingMasUpd.Where(x => x.BookingId == BookingID).FirstOrDefault();

                if (FetchBookingId != null)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {

                        FetchBookingId.Status = "C";

                        context.SaveChanges();
                        scope.Complete();

                        response.ErrorCode = ResponseStatus.Success;
                        response.ErrorMessage = ResponseMessages.Success;
                    }

                }

                response.ErrorCode = ResponseStatus.Success;
                response.ErrorMessage = ResponseMessages.Success;

            }

            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;
        }

        public Response<Object> GHBookingRequestedList()
        {
            var response = new Response<Object>();

            try
            {

                var GHBookingInfo = (
                   from GHBkInfor in context.GHBookingMasUpd
                   where (GHBkInfor.Approved == "N" && GHBkInfor.Status == "O")
                   select new GHBookingMasInputs
                   {

                       RowId = GHBkInfor.RowId,
                       BookingId = GHBkInfor.BookingId,
                       GuestName = GHBkInfor.GuestName,
                       GuestContactNo = GHBkInfor.GuestContactNo,
                       GuestEmail = GHBkInfor.GuestEmail,
                       DateFrom = GHBkInfor.DateFrom,
                       DateTo = GHBkInfor.DateTo,
                       NoofRooms = GHBkInfor.NoofRooms,
                       NoofPersons = GHBkInfor.NoofPersons,
                       Category = GHBkInfor.Category,
                       BookingName = GHBkInfor.BookingName,
                       StudOrStaffID = GHBkInfor.StudOrStaffID,
                       Designation = GHBkInfor.Designation,
                       BookingContactNo = GHBkInfor.BookingContactNo,
                       Department = GHBkInfor.Department,
                       BookingEmail = GHBkInfor.BookingEmail,
                       Food = GHBkInfor.Food,
                       Status = GHBkInfor.Status,
                       Approved = GHBkInfor.Approved,

                   }
               ).ToList();

                if (GHBookingInfo != null)
                {
                    response.Result = GHBookingInfo;
                    response.ErrorCode = ResponseStatus.Success;
                    response.ErrorMessage = ResponseMessages.Success;
                }

            }
            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;

        }

        public Response<object> GHBookingRequestedApproved(string BookingID, string Approval)
        {
            var response = new Response<object>();

            try
            {
                var FetchBookingId = context.GHBookingMasUpd.Where(x => x.BookingId == BookingID).FirstOrDefault();

                if (FetchBookingId != null)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {

                        FetchBookingId.Approved = Approval;

                        context.SaveChanges();
                        scope.Complete();

                        response.ErrorCode = ResponseStatus.Success;
                        response.ErrorMessage = ResponseMessages.Success;
                    }

                }

                response.ErrorCode = ResponseStatus.Success;
                response.ErrorMessage = ResponseMessages.Success;

            }

            catch (Exception e)
            {
                response.ErrorCode = ResponseStatus.Error;
                response.ErrorMessage = e.ToString();
            }
            return response;
        }
        // public Response<Object> GetGHApprovalList()
        //{
        //    var response = new Response<Object>();

        //    try
        //    {

        //        var GHBookingApprovalList = (
        //           from BookedRoom in context.RoomBookMasUpd
        //           where (BookedRoom.Approved == "N" && BookedRoom.Status == "O")
        //           select new GHBookMasInputs
        //           {

        //               RowId = BookedRoom.RowId,
        //               BookingId = BookedRoom.BookingId,
        //               BookName = BookedRoom.BookName,
        //               Rollno = BookedRoom.Rollno,
        //               RoomCount = BookedRoom.RoomCount,
        //               DateFrom = BookedRoom.DateFrom,
        //               DateTo = BookedRoom.DateTo,
        //               Status = BookedRoom.Status,
        //               Approved = BookedRoom.Approved
        //                                  }
        //       ).ToList();

        //        if (GHBookingApprovalList != null)
        //        {
        //            response.Result = GHBookingApprovalList;
        //            response.ErrorCode = ResponseStatus.Success;
        //            response.ErrorMessage = ResponseMessages.Success;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        response.ErrorCode = ResponseStatus.Error;
        //        response.ErrorMessage = e.ToString();
        //    }
        //    return response;

        //}

        //public Response<Object> GetGHApproval(String BookingId)
        //{
        //    var response = new Response<Object>();

        //    try
        //    {

        //        var GHBookingApprovalDetail = (
        //           from BookedRoom in context.RoomBookTranUpd
        //           where (BookedRoom.BookingId == BookingId && BookedRoom.Status == "O")
        //           select new GHBookTranInputs
        //           {

        //               RowId = BookedRoom.RowId,
        //               BookingId = BookedRoom.BookingId,
        //               RoomCount = BookedRoom.RoomCount,
        //               RoomId = BookedRoom.RoomId,
        //               Status = BookedRoom.Status

        //           }
        //       ).ToList();

        //        if (GHBookingApprovalDetail != null)
        //        {
        //            response.Result = GHBookingApprovalDetail;
        //            response.ErrorCode = ResponseStatus.Success;
        //            response.ErrorMessage = ResponseMessages.Success;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        response.ErrorCode = ResponseStatus.Error;
        //        response.ErrorMessage = e.ToString();
        //    }
        //    return response;

        //}

        private String GetLastBookingId()
        {
            //var result = context.GHBookingId.Where(x => x.id == "7").Select(x => (x.Value)).FirstOrDefault();
            var result = context.GHBookingId.Select(x => (x.BookID)).FirstOrDefault();

            var entitiesToUpdate = context.GHBookingId.Where(entity => entity.RowId == 1);

            // Update the column value of each entity
            foreach (var entity in entitiesToUpdate)
            {
                entity.BookID = Convert.ToInt32(result) + 1;
            }

            // Save the changes to the database
            context.SaveChanges();

            //int newNumber = int.Parse(result) + 1; 
            int newNumber = Convert.ToInt32(result) + 1;

            string formattedNumber = "NIITGH" + DateTime.Now.Year.ToString() +
                DateTime.Now.Month.ToString() + newNumber.ToString().PadLeft(5, '0');

            //return context.commonMaster.Where(x => x.id == "4");
            //return 0;
            return formattedNumber.Trim();
        }



        //private String sendEmail()
        //{
        //    var email = new MimeMessage();

        //    email.From.Add(new MailboxAddress("Sender Name", "sender@email.com"));
        //    email.To.Add(new MailboxAddress("Receiver Name", "receiver@email.com"));

        //    email.Subject = "Testing out email sending";
        //    email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        //    {
        //        Text = "<b>Hello all the way from the land of C#</b>"
        //    };

        //    using (var smtp = new SmtpClient())
        //    {

        //        smtp.Connect("smtp.gmail.com", 587, false);

        //        // Note: only needed if the SMTP server requires authentication
        //        smtp.Authenticate("smtp_username", "smtp_password");

        //        smtp.Send(email);
        //        smtp.Disconnect(true);
        //    }
        //    return "Ok";
        //}

        /// <summary>
        /// Used to update the JournalId next sequence number
        /// </summary>
        /// <param >JournalSequence</param>
        /// <returns></returns>
        //private void updateBookingIDSequence(String BookingId)
        //{

        //    var entitiesToUpdate = context.GHBookingId.Where(entity => entity.id == "7");

        //    foreach (var entity in entitiesToUpdate)
        //    {
        //        entity.Value = journalSequence;
        //    }

        //    // Save the changes to the database
        //    context.SaveChanges();
        //}



        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }


        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }



    }
}
