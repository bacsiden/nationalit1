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
});

