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
