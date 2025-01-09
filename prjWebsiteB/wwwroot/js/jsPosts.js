$("#searchString").on("keyup", function () {
    const searchString = encodeURIComponent($(this).val());
    $.ajax({
        type: "GET",
        url: `TPosts/Search/${searchString}`
    }).done(data => {
        $("#searchResults").html(data);
    }).fail(err => {
        alert(err.statusText);
    })
});

function editData() {
    var token = $('input[name="__RequestVerificationToken"]').val();
    var FPostId = $("#FPostIdforDetails").text();
    var FIsPublic = $("#FIsPublic").val();
    $.ajax({
        type: "POST",
        url: "/TPosts/Edit",
        data: { id: FPostId, isPublic: FIsPublic, searchString: $("#searchString").val(), page: $("#postsPage").attr("name"), __RequestVerificationToken: token }
    }).done(data => {
        $("#searchResults").html(data);
    }).fail(err => {
        alert(err.statusText);
    })
}

function deleteData() {
    var token = $('input[name="__RequestVerificationToken"]').val();
    var FPostId = $("#FPostIdforDelete").text();
    $.ajax({
        type: "POST",
        url: "/TPosts/Delete",
        data: { id: FPostId, searchString: $("#searchString").val(), __RequestVerificationToken: token }
    }).done(data => {
        $("#searchResults").html(data);
    }).fail(err => {
        alert(err.statusText);
    })
}

