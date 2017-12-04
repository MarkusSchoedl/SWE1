function parseToLowerText() {
    var txt = document.getElementById("tolower_text").value;
    console.log(txt);
    $.post("/to-lower", { text: txt }, function (result) {
        console.log(result);
        if (result.startsWith("text="))
            document.getElementById("tolower_textbox").innerHTML = result.substr(5, result.length - 5);
        else
            document.getElementById("tolower_textbox").innerHTML = result;
    });
}

function HoleOrte() {
    var txt = "street=" + document.getElementById("NaviStreet").value;
    console.log(txt);
    //$.post("/navigation", {street: ("street"+txt)}, function (result) {
    //    console.log(result);
    //    document.getElementById("NaviContent").innerHTML = result;
    //});
    var xhttp = new XMLHttpRequest();
    xhttp.open("POST", "/navigation", true);
    xhttp.setRequestHeader("Content-type", "text/plain");
    xhttp.send(txt);
    document.getElementById("NaviContent").innerHTML = xhttp.responseText;
}

function UpdateWholeMap() {
    var txt = document.getElementById("NaviStreet").value;
    $.post("/navigation?Update=true", null, function (result) {
        document.getElementById("NaviContent").innerHTML = result;
    });
}
