using System;
using System.Linq;
using Microsoft.Ajax.Utilities;
using static GoldenTicket.Models.EmployeeModel;

namespace GoldenTicket.Models
{
    /// <inheritdoc />
    /// <summary>
    ///     Modelo para el manejo de los tickets en el registro
    /// </summary>
    public class TicketRegisterResult : Result
    {
        /// <summary>
        ///     Id del nuevo ticket
        /// </summary>
        public long IdNewTicket { set; get; }

        /// <summary>
        ///     Sequencia del ticket nuevo
        /// </summary>
        public int SequenceNewTicket { set; get; }

        /// <summary>
        ///     Folio del ticket nuevo
        /// </summary>
        public string FolioNewTicket { set; get; }

        /// <summary>
        ///     Nombre completo del solicitante
        /// </summary>
        public string CustomerFullName { set; get; }

        /// <summary>
        ///     Correo del solicitante
        /// </summary>
        public string CustomerEmail { set; get; }

        /// <summary>
        ///     Descripcion del ticket
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        ///     Titulo o asunto del ticket
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        ///     Estatus del ticket
        /// </summary>
        public string Status { set; get; }

        /// <summary>
        ///     Persona asignada al ticket
        /// </summary>
        public string Employee { set; get; }

        /// <summary>
        ///     Notas adicionales del ticket
        /// </summary>
        public string Note { get; set; }
    }

    /// <summary>
    ///     Modelo para crear un nuevo ticket
    /// </summary>
    public class TicketRegisterModel
    {
        /// <summary>
        ///     Funcion para generar el id del ticket
        /// </summary>
        /// <returns>Un string con el numero de ticket</returns>
        public static string GenerateTicketFolio()
        {
            string ticketNumber;

            using (var db = new dbGoldenTicket())
            {
                var count = (
                                from tblTicketAux in db.TK_HD_TICKETS
                                join tblRecordAux in db.TK_DT_RECORDS on tblTicketAux.TK_HD_TICKETS_ID equals
                                    tblRecordAux.TK_HD_TICKETS_ID
                                where tblRecordAux.TK_CT_STATUS_ID == "ABT"
                                      && tblRecordAux.ACTIVITY_DATE.Year == DateTime.Today.Year
                                      && tblRecordAux.ACTIVITY_DATE.Month == DateTime.Today.Month
                                      && tblRecordAux.ACTIVITY_DATE.Day == DateTime.Today.Day
                                select tblTicketAux.TK_HD_TICKETS_ID
                            ).Distinct().Count() + 1;

                ticketNumber = DateTime.Now.ToString("yyyyMMdd") + count.ToString().PadLeft(4, '0');
            }

            return ticketNumber;
        }

        /// <summary>
        ///     Funcion para generar un nuevo id para el registro en el historial del ticket
        /// </summary>
        /// <param name="ticketId">Id del ticket a generar un nuevo registro</param>
        /// <returns>Id del nuevo registro para el ticket</returns>
        public static int GenerateRecordId(long ticketId)
        {
            int recordNumber;

            using (var db = new dbGoldenTicket())
            {
                recordNumber = (from tblRecordAux in db.TK_DT_RECORDS
                                   where tblRecordAux.TK_HD_TICKETS_ID == ticketId
                                   select tblRecordAux).Distinct().Count() + 1;
            }

            return recordNumber;
        }

        /// <summary>
        ///     Funcion que dados los datos de un ticket se crea un nuevo elemento
        ///     y su inicio en el historial
        /// </summary>
        /// <param name="ticketRecord">Objeto con los datos para el historial del ticket</param>
        /// <param name="ticket">objeto con los datos del ticket para su creacion </param>
        /// <returns>TicketRegisterResult con el folio y id del ticket nuevo</returns>
        public TicketRegisterResult CreateNewTicket(TK_HD_TICKETS ticket, TK_DT_RECORDS ticketRecord)
        {
            var result = new TicketRegisterResult();

            using (var db = new dbGoldenTicket())
            {
                db.TK_HD_TICKETS.Add(ticket);
                ticketRecord.TK_HD_TICKETS_ID = ticket.TK_HD_TICKETS_ID;
                ticketRecord.TK_DT_RECORDS_ID = GenerateRecordId(ticketRecord.TK_HD_TICKETS_ID);
                db.TK_DT_RECORDS.Add(ticketRecord);
                db.SaveChanges();

                result.FolioNewTicket = ticket.FOLIO;
                result.IdNewTicket = ticket.TK_HD_TICKETS_ID;

                result.Success = true;
                result.Message = "op_exitosa";
            }

            return result;
        }


        /// <summary>
        ///     Funcion para insertar un nuevo elemento en la tabla historial
        /// </summary>
        /// <param name="customerLocation">Ubicacion del solicitante</param>
        /// <param name="employeeName">Nombre del solicitante</param>
        /// <param name="ticketRecord">Nuevo objeto para ser insertado en la base de datos </param>
        /// <param name="ticketId">Id del ticket a actualizar</param>
        /// <param name="customerEmail">Correo del solicitante</param>
        /// <param name="customerExt">Extension del solicitante</param>
        /// <param name="customerPhone">Telefono del solicitante</param>
        /// <param name="customerArea">Area del solicitante</param>
        /// <returns>Success booleano</returns>
        public TicketRegisterResult UpdateTicket(long ticketId, string customerEmail, int customerExt,
            string customerPhone, string customerArea,
            string customerLocation, string employeeName, TK_DT_RECORDS ticketRecord)
        {
            var result = new TicketRegisterResult();

            using (var db = new dbGoldenTicket())
            {
                if (!ticketRecord.TK_BT_EMPLOYEES_ID.IsNullOrWhiteSpace())
                {
                    var employee = new TK_BT_EMPLOYEES
                    {
                        TK_BT_EMPLOYEES_ID = ticketRecord.TK_BT_EMPLOYEES_ID,
                        FULLNAME = employeeName
                    };

                    var employeeAux = new EmployeeRegister();
                    employeeAux.UpdateEmployee(employee);
                }

                // Actualizo la informacion de la tabla ticket
                var ticket = (
                    from tblTicket in db.TK_HD_TICKETS
                    where tblTicket.TK_HD_TICKETS_ID == ticketId
                    select tblTicket
                ).SingleOrDefault();

                if (ticket != null)
                {
                    ticket.CUSTOMER_EMAIL = customerEmail;
                    ticket.CUSTOMER_EXTENSION = customerExt;
                    ticket.CUSTOMER_AREA = customerArea;
                    ticket.CUSTOMER_PHONE = customerPhone;
                    ticket.CUSTOMER_LOCATION = customerLocation;

                    db.SaveChanges();

                    result.CustomerEmail = ticket.CUSTOMER_EMAIL;
                    result.CustomerFullName = ticket.CUSTOMER_FULLNAME;
                    result.FolioNewTicket = ticket.FOLIO;
                    result.Content = ticket.CONTENT;
                    result.Title = ticket.TITLE;
                    result.IdNewTicket = ticket.TK_HD_TICKETS_ID;

                    ticketRecord.TK_DT_RECORDS_ID = GenerateRecordId(ticketRecord.TK_HD_TICKETS_ID);
                    db.TK_DT_RECORDS.Add(ticketRecord);

                    result.Status = ticketRecord.TK_CT_STATUS_ID;
                    result.Employee = ticketRecord.TK_BT_EMPLOYEES_ID;
                    result.Note = ticketRecord.NOTE;

                    db.SaveChanges();
                }

                result.Success = true;
                result.Message = "op_exitosa";
            }

            return result;
        }

        /// <summary>
        ///     Agrega un nuevo registro en el historial de un ticket
        /// </summary>
        /// <param name="ticketRecord">Objeto con la informacion a agregar en el historial del ticket</param>
        /// <returns>TicketRegisterResult con la secuencia del nuevo registro</returns>
        public TicketRegisterResult UpdateRecord(TK_DT_RECORDS ticketRecord)
        {
            var result = new TicketRegisterResult();

            using (var db = new dbGoldenTicket())
            {
                ticketRecord.TK_DT_RECORDS_ID = GenerateRecordId(ticketRecord.TK_HD_TICKETS_ID);
                db.TK_DT_RECORDS.Add(ticketRecord);
                db.SaveChanges();

                result.SequenceNewTicket = ticketRecord.TK_DT_RECORDS_ID;
                result.Success = true;
                result.Message = "op_exitosa";
            }

            return result;
        }
    }
}