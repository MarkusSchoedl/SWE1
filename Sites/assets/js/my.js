//$(document).ready(function () {
//    $(function () {
//        $('#DatePickerFrom').datetimepicker({
//            viewMode: 'days',
//            format: 'DD/MM/YYYY'
//        });
//    });
//});

function parseToLowerText() {
    var txt = document.getElementById("tolower_text").value;

    $.post("/to-lower", { text: txt }, function (result) {
        console.log(result);
        if (result.startsWith("text="))
            document.getElementById("tolower_textbox").innerHTML = result.substr(5, result.length - 5);
        else
            document.getElementById("tolower_textbox").innerHTML = result;
    });
}

function HoleOrte() {
    document.getElementById("NaviContent").innerHTML = "<div class='loader'></div>Loading ...";
    var txt = document.getElementById("NaviStreet").value;
    console.log(txt);
    $.post("/navigation", { street: txt }, function (result) {
        document.getElementById("NaviContent").innerHTML = result;
    });
}

function UpdateWholeMap() {
    document.getElementById("NaviContent").innerHTML = "<div class='loader'></div>Loading ...";
    var txt = document.getElementById("NaviStreet").value;
    $.post("/navigation?Update=true", null, function (result) {
        document.getElementById("NaviContent").innerHTML = result;
    });
}


function SwitchTempPage(arrow) {
    var PageNumElement = document.getElementById("TempPageNum");
    console.log(PageNumElement.innerHTML);
    if (arrow === "right" && document.getElementById("TempContent").innerHTML) {
        GetTemperatureData(parseInt(PageNumElement.innerHTML) + 1);
    }
    else if (arrow === "left" && document.getElementById("TempContent").innerHTML) {
        if (parseInt(PageNumElement.innerHTML) > 1) {
            GetTemperatureData(parseInt(PageNumElement.innerHTML) - 1);
        }
    }
}

function GetTemperatureData(page = "1") {
    var fromTxt = document.getElementById("DatePickerFrom").value;
    var toTxt = document.getElementById("DatePickerTo").value;

    document.getElementById("TempPageNum").innerHTML = page;

    $.post("/temperature/" + fromTxt + "/" + toTxt + "/?page=" + page, {}, function (result) {
        result = result.replace("<table>", "<table style='width: 100%; border: 1px white;'>");
        result = result.replace("<tr>", "<tr style='width: 100%; '>");
        result = result.replace("<td>", "<td style='width: 50%; '>");
        document.getElementById("TempContent").innerHTML = result;
    });
}
