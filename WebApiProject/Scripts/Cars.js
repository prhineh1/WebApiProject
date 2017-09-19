


function getCars(url, year, make, model) {
    return $.get('/api/Cars/' + url, {year: year, make: make, model: model});
}


var year;
var make;
var model;
function MakeDropdown(selector, data) {
    for (i = 0; i < data.length; i++) {
        $(selector).append("<option value='" + data[i] + "'>" + data[i] + "</option>")
    }
}

getCars('GetModelYears').done(function (data) {
    MakeDropdown('#makes-for-year', data)
})

$('#makes-for-year').change(function () {
    if ($(this).val() != 0) {
        $('#car-makes').html('<option value="0"></option>');
        $('#car-models').empty();
        year = $(this).val()
        getCars('GetMakesForYear', year).done(function (data) {
            MakeDropdown('#car-makes', data);
            $('#car-makes').prop('disabled', false);
        })
    }
})

$('#car-makes').change(function () {
    if ($(this).val() != 0) {
        $('#car-models').empty();
        make = $(this).val()
        getCars('GetModelsForYearAndMake', year, make).done(function (data) {
            MakeDropdown('#car-models', data);
            $('#car-models').prop('disabled', false);
        })
    }
})


var queryString;
$('#find-cars').click(function () {
    model = $('#car-models').val();
    getCars('GetCarsByYearAndMakeAndModel', year, make, model).done(function (data) {
        queryString = data[0].model_year + '+' + data[0].make_display + '+' + data[0].model_name;
        ImageSearch()
    })
})

function ImageSearch() {
    $.get('/api/Cars/GetGoogleImages', {year: year, make: make, model: model}).done(function (data) {
        $('#google-search').empty();
        for (i = 0; i < data.items.length; i++) {
            $('#google-search').append('<a href="' + data.items[i].image.contextLink + '"><img src="' + data.items[i].image.thumbnailLink + '"></a></br>')
        }
    })
    //$.get('https://www.googleapis.com/customsearch/v1', { key: 'AIzaSyD1Ti8_nUHzamUKdqlJ6hmx3zRnUdVKpjY', cx: '012019432613858739685:wgufdxmecnk', searchType: 'image', q: queryString }).done(function (data) {
    //    $('#google-search').empty();
    //    for (i = 0; i < data.items.length; i++) {
    //        $('#google-search').append('<a href="' + data.items[i].image.contextLink + '"><img src="' + data.items[i].image.thumbnailLink + '"></a></br>')
    //    }
    //})
    RecallInfo()
}

function RecallInfo() {
    $.get('/api/Cars/GetRecallInfo', { year: year, make: make, model: model }).done(function (data) {
        alert(data);
    })
    //$.get('https://one.nhtsa.gov/webapi/api/Recalls/vehicle/modelyear/' + year + '/make/' + make + '/model/' + model, { format: 'json' }).done(function (data) {
    //    $('#carlist').empty();
    //})
    //$.ajax({
    //    url: 'https://one.nhtsa.gov/webapi/api/Recalls/vehicle/modelyear/' + year + '/make/' + make + '/model/' + model,
    //    data: {
    //        format: 'json'
    //    },
    //    crossDomain: true,
    //    dataType: 'json',
    //    method: 'GET',
    //    xhrFields: {
    //        withCredentials: true
    //    },
    //    success: function (response) {
    //        alert('it worked.')
    //    }
    //}).done(function (data) {
    //    $('#carlist').empty();
    //})
}

