function loadTicketDetails() {
    $(".fixed-action-btn").floatingActionButton();
    $(".fixed-action-btn").show("slow");
    $('[data-toggle="tooltip"]').tooltip();
    document.getElementById("modalAceptNew").style.display = "none";

    const url = document.getElementById("allTicketData").value;

    window.hasRun = true;

    $.ajax({
        url: url,
        type: "POST",
        dataType: "json",
        success: function (data) {

            $("#StatusDiv select").val(data.Status);

            //Validacion cuando esta TERMINADO se puede pasar a ASIGNADO/EN PROCESO
            if (data.Status == 'TMN') {

                $("#StatusDiv select option[value*='ABT']").prop("disabled", true);
                $("#StatusDiv select option[value*='CDO']").prop("disabled", true);
                $("#StatusDiv select option[value*='ESU']").prop("disabled", true);
            } 

        },
        error: function() {
            alert("Ocurrió un error al cargar los datos, porfavor inténtalo de nuevo.");
        }
    });

}