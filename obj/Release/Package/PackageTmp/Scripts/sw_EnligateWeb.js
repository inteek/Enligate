//var messagesTimeout;
function Enligate_onPageLoad() {
    switchButtons();
    switchDates();
    mUploadFiles();
}

function initializeRating() {
    $(".trophyRating").each(function () {
        var val = $(this).data("rating");
        var size = Math.max(0, (Math.min(5, val))) * 18;
        var $span = '<span style="width:' + size + 'px;"></span>';
        $(this).html($span);
    });
}
function validateEmail(email) {
    console.log("**DEBUG**"+email)
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function validateName(input) {
    var str = $.trim($(input).val());
    if (str != "") {
        var regx = /^[a-zA-Z\u00C0-\u024F]+$/;
        if (!regx.test(str)) {
            $(".infoUser").html("Alphanumeric only allowed !");
            return false;
        }
        else {
            $(".infoUser").html("");
            return true;
        }
    }
    else {
        //empty value -- do something here
        $(".infoUser").html("");
        return true;
    }
}

function showMessages() {
    var msgFadeOutDelay = 2000;
    var msgFadeOutTime = 3500;

    $(".m-messageHide").each(function () {
        var thisDiv = $(this).html();
        //alert($(this).html());
        if (thisDiv != null && thisDiv != undefined && thisDiv.trim() != "") {
            var msgShow = $(".m-messageShow");
            msgShow.html("");
            msgShow.stop(true, false).fadeIn();
            msgShow.html(thisDiv);
            msgShow.mouseover(function () {
                msgShow.stop(true, false).fadeIn();
            });
            msgShow.mouseout(function () {
                msgShow.stop(true, false)
                       .delay(msgFadeOutDelay)
                       .fadeOut({
                           duration: msgFadeOutTime,
                           complete: function () {
                               msgShow.html("");
                           }
                       });
            });
            $(this).html("");

            //Comienza a ocultar el mensaje
            msgShow.delay(msgFadeOutDelay)
                   .fadeOut({
                       duration: msgFadeOutTime,
                       complete: function () {
                           msgShow.html("");
                       }
                   });
            return;
        }
    });
}

function hideMessage() {
    //$(".m-messageShow").html("");
}

function switchButtons(clickButton) {
    $(".mSwitchBtns").each(function () {
        var inputRes = $(this).attr("data-relClass");
        var inputVal = $(this).attr("data-val");
        var inputHid = $('input[data-relClass="' + inputRes + '"]');
        
        
        $(this).on("click", function () {
            if (inputHid.val() != inputVal)
            {
                inputHid.val(inputVal);
                var activeButton = $('.swActive[data-relClass="' + inputRes + '"]');
                activeButton.removeClass("swActive");
                $(this).addClass("swActive");
                inputHid.change();
            }
        });

        var defaultVal = inputHid.val();
        if (defaultVal == "") {
            defaultVal = inputHid.attr("data-defaultVal");
        }
        if (defaultVal == inputVal && clickButton == undefined) {
            $(this).click();
            $(this).addClass("swActive");
           // console.log("Hola button");
        } else {
           // console.log("que pasa aqui"+inputVal);
        }
    });
}

function switchDates() {
    $(".mSwitchDate").each(function () {
        var inputRel = $(this).attr("data-relClass");
        var inputHid = $('input[data-relClass="' + inputRel + '"]');

        //Actualiza los select box con la fecha actual
        var defaultVal = inputHid.val();
        if (defaultVal != "") {
            var day = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="dd"]');
            var month = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="MM"]');
            var year = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="yyyy"]');

            var date = defaultVal.split("/");
            day.val(date[0]);
            month.val(date[1]);
            year.val(date[2]);
        }

        $(this).on("change", function () {
            var day = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="dd"]');
            var month = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="MM"]');
            var year = $('.mSwitchDate[data-relClass="' + inputRel + '"][data-type="yyyy"]');

            if (day.val() != "" && month.val() != "" && year.val() != "") {
                var date = day.val() + "/" + month.val() + "/" + year.val();
                if (inputHid.val() != date) {
                    inputHid.val(date);
                    inputHid.change();
                }
            }
            else {
                if (inputHid.val() != "") {
                    inputHid.val("");
                    inputHid.change();
                }
            }
        });
    });
}

function mUploadFiles() {
    $("input.mUploadFile").each(function () {
        var inputLoadImage = $(this).attr("data-LoadImg");
        var inputSubmitForm = $(this).attr("data-submitFormFunction");
        var inputRel = $(this).attr("data-rel");
        var inputFile = $(this);
        var imgGlyph = $('span[data-rel="' + inputRel + '"]');
        var imgImage = $('img[data-rel="' + inputRel + '"]');
        var btnSubmit= $('button[data-rel="' + inputRel + '"]');

        imgGlyph.on("click", function () {
            inputFile.click();
        });

        inputFile.change(function () {
            if (inputLoadImage == "true") {
                readImageURL(this, imgImage);
            }

            //hace el submit si tiene que hacerlo en onchange.
            if(inputSubmitForm != undefined)
            {
                eval(inputSubmitForm + "()");
            }
        });
    });
}

function readImageURL(input, image) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            image.attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }
}

function submitForm(nameID)
{
    nameID = "#" + nameID;
    $(nameID).submit();
}

function formInlineResize(maxWidth, cssClassContainer) {
    var cssContainer = "." + cssClassContainer;
    var divC = $(cssContainer);
    if ($(window).width() > 750) {
        if(divC.length > 0)
            divC.width(divC.parent().width() - 240);
    }

    $(".form-inline").each(function (index, element) {
        var divFormGroup = $(this).children("div.form-group").first();
        var label = divFormGroup.children(".mLabelGroup").first();
        var input = divFormGroup.children(".mInputGroup").first();

        var br = divFormGroup.children("br:visible").first();
        var padding = 15;
        if (br.length == 0) {
            if (maxWidth <= $(window).width()) {
                var inputSize = divFormGroup.width() - label.width() - padding - 3;
                if (inputSize > 0) {
                    input.width(inputSize);
                }
            }
            if (divFormGroup.hasClass("mSpaceMb0")) {
                divFormGroup.removeClass("mSpaceMb0");
                divFormGroup.addClass("mSpaceMb5");
            }
        }
        else {
            if (maxWidth <= $(window).width()) {
                var inputSize = divFormGroup.width() - padding - 3;
                if (inputSize > 0) {
                    input.width(inputSize);
                }
            }
            if (divFormGroup.hasClass("mSpaceMb5")) {
                divFormGroup.removeClass("mSpaceMb5");
                divFormGroup.addClass("mSpaceMb0");
            }
        }
    });
}

var formFilled = false;
function dontCloseWindow(msg) {
    if (!formFilled){
        formFilled = true;

        var myEvent = window.attachEvent || window.addEventListener;
        var chkevent = window.attachEvent ? 'onbeforeunload' : 'beforeunload'; /// make IE7, IE8 compitable

        myEvent(chkevent, function (e) { // For >=IE7, Chrome, Firefox
            if (formFilled) {
                (e || window.event).returnValue = msg;
                return msg;
            }
        });
    }
}

function loadGoogleMaps() {

    $(".googleMap").each(function () {
        var divId = $(this).attr("id");
        alert(divId);
        
        div = div.replace("mapGoogle", "initMap");
        alert(divId);
    });

}

function onlyDecimals(key, txt) {
    var keycode = (key.which) ? key.which : key.keyCode
    //comparing pressed keycodes numbers or .
    //alert(keycode);
    if ((keycode > 47 && keycode < 58) || keycode == 46) {
        var actualValue = txt.value + String.fromCharCode(keycode);
        var ex = /^[0-9]+\.?[0-9]*$/;
        if (ex.test(actualValue) == false) {
            return false;
        }
        else {
            return true;
        }
    }
        //comparing pressing del, left arrow, right arrow or tab
    else if (keycode == 8 || keycode == 37 || keycode == 39 || keycode == 9) {
        return true;
    }
    else {
        return false;
    }
}

function onlyNumbers(key, txt) {
    var keycode = (key.which) ? key.which : key.keyCode
    //comparing pressed keycodes numbers or .
    //alert(keycode);
    if ((keycode > 47 && keycode < 58)) {
        return true;
    }
        //comparing pressing del, left arrow, right arrow or tab
    else if (keycode == 8 || keycode == 37 || keycode == 39 || keycode == 9) {
        return true;
    }
    else {
        return false;
    }
}

function disableEnterKey(e) {
    var key;

    if (window.event)
        key = window.event.keyCode;     //IE
    else
        key = e.which;     //firefox

    if (key == 13)
        return false;
    else
        return true;
}

function getCleanedString(cadena) {
    // Definimos los caracteres que queremos eliminar
    var specialChars = "!@#$^&%*()+=-[]\/{}|:<>?,.";

    // Los eliminamos todos
    for (var i = 0; i < specialChars.length; i++) {
        cadena = cadena.replace(new RegExp("\\" + specialChars[i], 'gi'), '');
    }

    // Lo queremos devolver limpio en minusculas
    cadena = cadena.toLowerCase();

    // Quitamos espacios y los sustituimos por _ porque nos gusta mas asi
    //cadena = cadena.replace(/ /g, "_");

    // Quitamos acentos y "ñ". Fijate en que va sin comillas el primer parametro
    cadena = cadena.replace(/á/gi, "a");
    cadena = cadena.replace(/é/gi, "e");
    cadena = cadena.replace(/í/gi, "i");
    cadena = cadena.replace(/ó/gi, "o");
    cadena = cadena.replace(/ú/gi, "u");
    cadena = cadena.replace(/ñ/gi, "n");
    cadena = cadena.replace(/à/gi, "a");
    cadena = cadena.replace(/è/gi, "e");
    cadena = cadena.replace(/ì/gi, "i");
    cadena = cadena.replace(/ò/gi, "o");
    cadena = cadena.replace(/ù/gi, "u");
    
    return cadena;
}