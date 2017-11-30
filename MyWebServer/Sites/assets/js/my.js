

function parseToLowerText() {
    var txt = document.getElementById("tolower_text").value;
    console.log(txt);
    $.post("/to-lower", { text: txt }, function (result) {
        console.log(result);
        $("tolower_textbox").html(result);
    });
    //var text = document.getElementById("tolower_text").value;
    //var textbox = document.getElementById("tolower_textbox");

    //console.log(text);

    //$.post("/to-lower", function (data, status) {
    //    alert("Data: " + data + "\nStatus: " + status);
    //});

    
    //$.ajax({
    //    type: 'POST',
    //    url: '/to-lower',
    //    data: 'text=' + text,
    //    success: function (msg) {
    //        textbox.innerHTML = msg;
    //    },
    //    error: function (msg) {
    //        console.dir(msg);
    //    }
    //});
}