angular.module('Directives.TableParse', [])
    .directive('tableParse', ['$parse', '$document',
        function ($parse, $document) {
            return {
                restrict: 'E',
                link: function (scope, element, attrs) {
                    var inFocus = false;
                    function parseTabular(text) {
                        //The array we will return
                        var toReturn = [];
                        try {
                            //Pasted data split into rows
                            var rows = text.split(/[\n\f\r]/);
                            rows.forEach(function (thisRow) {
                                var row = thisRow.trim();
                                if (row != '') {
                                    var cols = row.split("\t");
                                    toReturn.push(cols);
                                }
                            });
                        }
                        catch (err) {
                            console.log('error parsing as tabular data');
                            console.log(err);
                            return null;
                        }
                        return toReturn;
                    }
                    function textChanged() {
                        var text = $('#myPasteBox').val();
                        if (text != '') {
                            //We need to change the scope values
                            scope.$apply(function () {
                                if (attrs.ngModel != undefined && attrs.ngModel != '') {
                                    $parse(attrs.ngModel).assign(scope, text);
                                }
                                if (attrs.ngArray != undefined && attrs.ngArray != '') {
                                    var asArray = parseTabular(text);
                                    if (asArray != null) {
                                        $parse(attrs.ngArray).assign(scope, asArray);
                                    }
                                }
                            });
                        }
                    }
                    $document.ready(function () {
                        //Handles the Ctrl + V keys for pasting
                        function handleKeyDown(e, args) {
                            if (!inFocus && e.which == keyCodes.V && (e.ctrlKey || e.metaKey)) {    // CTRL + V
                                //reset value of our box
                                $('#myPasteBox').val('');
                                //set it in focus so that pasted text goes inside the box
                                $('#myPasteBox').focus();
                            }
                        }
                        //We add a text area to the body only if it is not already created by another myPaste directive
                        if ($('#myPasteBox').length == 0) {
                            $('body').append($('<textarea id=\"myPasteBox\" style=\"position:absolute; left:-1000px; top:-1000px;\"></textarea>'));
                            var keyCodes = {
                                'C': 67,
                                'V': 86
                            };
                            $('body').on("focus", 'input, textarea', function () {
                                //If this is true, we wont respond to Ctrl + V
                                inFocus = true;
                            });
                            $('body').on("blur", 'input, textarea', function () {
                                //We are not on a text element so we will respond
                                //to Ctrl + V
                                inFocus = false;
                            });
                            //Handle the key down event
                            $(document).keydown(handleKeyDown);
                            //We will respond to when the textbox value changes
                            $('#myPasteBox').bind('input propertychange', textChanged);
                        }
                        else {
                            //We will respond to when the textbox value changes
                            $('#myPasteBox').bind('input propertychange', textChanged);
                        }
                    });
                }
            }
        }]);
