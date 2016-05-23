var model = {};


model.Initialize = initialize;

function initialize() {

    $.ajax({
        method: "POST",
        url: "/Home/Initialize"
    }).done(function (response) {
        ko.mapping.fromJS(response, null, model);

        if (ko.applied === undefined) {

            model.ZipIsActive = ko.computed(function () {
                if (model.SearchCity() && model.SearchState()) {
                    return false;
                }
                return true;
            });

            model.CityStateIsActive = ko.computed(function () {
                if (model.SearchZip()) {
                    return false;
                }
                return true;
            });

            ko.applyBindings(model);
        }

        ko.applied = true;


    }).fail(function () {
        alert("There was a problem connecting to Server");
    });
}



model.Search = function () {
    if (validateInfo()) {
        $.ajax({
            method: "POST",
            url: "/Home/SearchAddress",
            contentType: "application/json",
            data: ko.toJSON(model)
        }).done(function (response) {
            ko.mapping.fromJS(response, null, model);

            if (ko.applied === undefined)
                ko.applyBindings(model);

            ko.applied = true;

            if (model.Errors().length === 0) {
                $('#Result').modal('show');
            }
            else {
                $('#Error').modal('show');
            }

        }).fail(function () {
            alert("There was a problem connecting to Server");
        });
    }
    else {
        model.Errors.push({
            ErrorCode: undefined,
            ErrorMessage: "City and State must be provided, Or Zip Code"
        });

        $('#Error').modal('show')
    }
};


function validateInfo() {

    if (model.Errors().length > 0) {
        model.Errors([]);
    }
    
    if(!model.SearchCity() || !model.SearchState())
    {
        if(!model.SearchZip())
        {
            return false;
        }
    }

    return true;
}

function onEnter() {
    model.Search();
}

ko.bindingHandlers.enterKey = {
    init: function (element, valueAccessor, allBindings, viewModel) {
        var callback = valueAccessor();
        $(element).keypress(function (event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            if (keyCode === 13) {
                callback.call(viewModel);
                return false;
            }
            return true;
        });
    }
};

$(initialize);