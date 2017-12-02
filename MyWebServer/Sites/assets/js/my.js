

function parseToLowerText() {
    var txt = document.getElementById("tolower_text").value;
    console.log(txt);
    $.post("/to-lower", { text: txt }, function (result) {
        console.log(result);
        if (result.startsWith("text="))
            document.getElementById("tolower_textbox").innerHTML = result.substr(5, result.length-5);
        else
            document.getElementById("tolower_textbox").innerHTML = result;
    });
}
