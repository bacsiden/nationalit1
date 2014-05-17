$(document).ready(function () {
    // thêm thẻ <li> cho phân trang
    $(".phantrangmvcpager a").wrap("<li></li>");
    $(".phantrangmvcpager li.active").wrapInner("<a href='#' onclick='return false;'></a>");
    $(".phantrangmvcpager a[disabled='disabled']").parent().addClass('disabled');

    $(".confirmDelete").click(function () {
        if (confirm("Are you sure?")) {
            return true;
        }
        return false;
    });
    $("#SubmitRole").click(function () {
        var idlist = "";
        $("#ListRoles input:checkbox:checked").each(function () {
            idlist += $(this).val() + ",";
        });
        $("#HiddenListRole").val(idlist);
    })
    $(".IsActive").click(function () {
        var $this = $(this);
        var id = $this.attr("getID");
        $.ajax({
            type: "post",
            url: "/Menu/DoActive",
            data: "id=" + id,
            success: function (kq) {
                $this.html(kq);
                location.href = "";
            }
        });
    });

    // show hide button Edit & Delete when click checkbox
    $(".checkAll").click(function () {
        $(".EditItem").hide();
        if ($(this).is(':checked')) {
            $(".checkitem").prop('checked', true);
            $(".DeleteItem").text('Delete All');
            $(".DeleteItem").show();
        } else {
            $(".checkitem").prop('checked', false);
            $(".DeleteItem").text('Delete');
            $(".DeleteItem").hide();
        }
    });
    $(".checkitem").click(function () {
        $(".DeleteItem").show();
        var numberOfChecked = $('input:checkbox:checked').length;
        if (numberOfChecked == 0) {
            $(".DeleteItem").hide();
        }
        if (numberOfChecked == 1) {
            $(".EditItem").show();
        } else {
            $(".EditItem").hide();
        }
    });

    // Event click with button Edit & Delete
    $(".DeleteItem").click(function () {
        var listID = "";
        var url = $(this).attr('href');
        $('input.checkitem:checkbox:checked').each(function () {
            listID += "," + $(this).val();
        });
        if (listID != "") {
            $(this).attr('href', url + "?arrayID=" + listID);
        } else {
            alert('Warring!');
            return false;
        }

    });

    $(".EditItem").click(function () {
        var obj = $('input.checkitem:checkbox:checked');
        if (obj.length == 1) {
            var value = obj.val();
            var url = $(this).attr('href');
            $(this).attr('href', url + "/" + value);
        } else {
            alert('Warring!');
            return false;
        }
    });

    // input DatePicker
    //    $('.date-picker').datepicker({ language: "en", format: "mm/dd/yyyy" }).on('changeDate', function (ev) {
    //        $('.date-picker').datepicker('hide');
    //    });       

});

// autocomplete 
function autoCompleteByClassName(className1, dataJson) {
    className1.typeahead({
        source: function (query, process) {
            objects = [];
            map = {};
            var data = dataJson // Or get your JSON dynamically and load it into this variable
            $.each(data, function (i, object) {
                map[object.label] = object;
                objects.push(object.label);
            });
            process(objects);
        },
        updater: function (item) {
            this.$element.attr('value', map[item].id)
            return item;
        }
    });
}

 