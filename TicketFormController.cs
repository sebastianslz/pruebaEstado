[HttpPost]
public JsonResult UpdateTicketFromDetails()
        {
            var ticketStatusInput = Request.Form["ticketStatusInput"];
        try
            {
                TicketRegisterResult result;               
                using (var scope = new TransactionScope())
                {
                    // I create a new record variable with all fields
                    var record = new TK_DT_RECORDS
                    {
                        TK_CT_STATUS_ID = ticketStatusInput,
                    };
                    // We update the ticket data (this will always be done)
                    var model = new TicketRegisterModel();
                    // We create the new record in the record table and insert it
                    result = model.UpdateTicket;

                    //If the ticket was not saved, the transaction is finished and we return the error message
                    if (!result.Success)
                        return Json(new TicketResult
                        {
                            IsValid = false,
                            Error = "The changes could not be saved, please try again."
                        });
                    scope.Complete();
                }   
            }catch (DbEntityValidationException ex)
        {
            //Falló al tratar de registrar datos en la base de datos
            foreach (var e in ex.EntityValidationErrors)
            foreach (var validationError in e.ValidationErrors)
                Console.WriteLine("Property: " + validationError.PropertyName + " Error: " +
                                  validationError.ErrorMessage);

            return Json(new TicketResult
            {
                IsValid = false,
                Error = "Ocurrió un error al crear el ticket, por favor inténtalo de nuevo."
            });
        }
            return Json(new TicketResult
            {
                IsValid = true
            });
            }