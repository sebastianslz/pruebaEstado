using System;
using System.Linq;
using System.Data.Entity.Validation;

namespace GoldenTicket.Models
{
    /// <summary>
    ///     Objeto historial del ticket
    /// </summary>
    public class TicketsRecord
    {
        /// <summary>
        ///     Id del historial
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        ///     Fecha de insercion del historico
        /// </summary>
        public DateTime ActivityDate { get; set; }

        /// <summary>
        ///     Descripcion del historico
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     Id del ticket relacionado al historial
        /// </summary>
        public long TicketId { get; set; }

        /// <summary>
        ///     Nota que se le agrega a realizar una accion
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        ///     Estatus del ticket para ese historico
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Tecnico asignado al ticket
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        ///     Usuario que esta logueado y realiza la accion de la modificacion
        /// </summary>
        public string UserUpdate { get; set; }

        /// <summary>
        ///     Id del servicio asociado al ticket
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        ///     Id del subservicio asociado al ticket
        /// </summary>
        public int SubServiceId { get; set; }

        /// <summary>
        ///     Prioridad del ticker
        /// </summary>
        public string PrioritiesId { get; set; }

        /// <summary>
        ///     Categoria asiociada al ticket
        /// </summary>
        public string CategoryId { get; set; }

        public string CustomerEmail{ get; set; }
        public string CustomerFullName{ get; set; }

        /// <summary>
        ///     Instanciar un objeto desde la informacion en la base de datos
        /// </summary>
        /// <param name="tkDtRecords">Objeto tipo tabla para obtener la infomacion desde la base de datos</param>
        public void FillRecord(TK_DT_RECORDS tkDtRecords)
        {
            RecordId = tkDtRecords.TK_DT_RECORDS_ID;
            ActivityDate = tkDtRecords.ACTIVITY_DATE;
            Content = tkDtRecords.CONTENT;
            TicketId = tkDtRecords.TK_HD_TICKETS_ID;
            Note = tkDtRecords.NOTE;
            UserUpdate = tkDtRecords.USER_UPDATE;
            Status = tkDtRecords.TK_CT_STATUS_ID;
            EmployeeId = tkDtRecords.TK_BT_EMPLOYEES_ID;
            ServiceId = tkDtRecords.TK_CT_SERVICES_ID;
            PrioritiesId = tkDtRecords.TK_CT_PRIORITIES_ID;
            CategoryId = tkDtRecords.TK_CT_CATEGORIES_ID;
        }

        /// <summary>
        ///     Instanciar un objeto tabla de base de datos desde un objeto historial
        /// </summary>
        /// <param name="tkDtRecords">Referencia a el elemento de la base de datos</param>
        public void FillRecordsDb(ref TK_DT_RECORDS tkDtRecords)
        {
            //tkDtRecords.TK_DT_RECORDS_ID = RecordId;
            tkDtRecords.ACTIVITY_DATE = ActivityDate;
            tkDtRecords.CONTENT = Content;
            tkDtRecords.TK_HD_TICKETS_ID = TicketId;
            tkDtRecords.NOTE = Note;
            tkDtRecords.TK_CT_STATUS_ID = Status;
            tkDtRecords.TK_BT_EMPLOYEES_ID = EmployeeId;
            tkDtRecords.USER_UPDATE = UserUpdate;
            tkDtRecords.TK_CT_SERVICES_ID = ServiceId;
            tkDtRecords.TK_CT_PRIORITIES_ID = PrioritiesId;
            tkDtRecords.TK_CT_CATEGORIES_ID = CategoryId;
        }
    }

    /// <summary>
    ///     Modelo para el listado de tickets
    /// </summary>
    public class TicketsModel
    {
        /// <summary>
        ///     Valida si un ticket le pertenece a la persona que lo esta consultando
        /// </summary>
        /// <param name="ticketId">Id del ticket a validar</param>
        /// <param name="userId">Usuario a validar</param>
        /// <returns>Booleano que indica si al usuario le pertenece el ticket</returns>
        public bool ValidateTicketOwner(long ticketId, string userId)
        {
            var result = false;

            try
            {
                using (var db = new dbGoldenTicket())
                {
                    var query = from tblTicket in db.TK_HD_TICKETS
                        join tblRecord in db.TK_DT_RECORDS on tblTicket.TK_HD_TICKETS_ID equals tblRecord
                            .TK_HD_TICKETS_ID
                        where tblTicket.TK_HD_TICKETS_ID == ticketId
                              && tblRecord.TK_BT_EMPLOYEES_ID == userId
                              && tblRecord.TK_DT_RECORDS_ID == (
                                  from tblTicketAux in db.TK_HD_TICKETS
                                  join tblRecordAux in db.TK_DT_RECORDS on tblTicketAux.TK_HD_TICKETS_ID equals
                                      tblRecordAux
                                          .TK_HD_TICKETS_ID
                                  where tblTicket.TK_HD_TICKETS_ID == tblTicketAux.TK_HD_TICKETS_ID
                                  select tblRecordAux.TK_DT_RECORDS_ID
                              ).Max()
                        select tblTicket;

                    result = query.Any();
                }
            }
            catch (DbEntityValidationException ex)
            {
                //Falló al tratar de registrar datos en la base de datos
                foreach (var e in ex.EntityValidationErrors)
                foreach (var validationError in e.ValidationErrors)
                    Console.WriteLine("Property: " + validationError.PropertyName + " Error: " +
                                      validationError.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        ///     Metodo para validar si un area puede ver cierto ticket
        /// </summary>
        /// <param name="ticketId">Id del ticket a validar</param>
        /// <param name="area">Area a validar</param>
        /// <returns>Booleano que indica si el ticket le pertence al area</returns>
        public bool ValidateTicketArea(long ticketId, string area)
        {
            var result = false;

            try
            {
                using (var db = new dbGoldenTicket())
                {
                    var query = from tblTicket in db.TK_HD_TICKETS
                        where tblTicket.TK_HD_TICKETS_ID == ticketId
                              && tblTicket.TK_CT_AREAS_ID == area
                        select tblTicket;

                    result = query.Any();
                }
            }
            catch (DbEntityValidationException ex)
            {
                //Falló al tratar de registrar datos en la base de datos
                foreach (var e in ex.EntityValidationErrors)
                foreach (var validationError in e.ValidationErrors)
                    Console.WriteLine("Property: " + validationError.PropertyName + " Error: " +
                                      validationError.ErrorMessage);
            }

            return result;
        }
    }
}