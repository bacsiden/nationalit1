$(document).ready(function () {
    $(".confirmDelete").click(function () {
        if (confirm("Bạn chắc chắn?")) {
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

