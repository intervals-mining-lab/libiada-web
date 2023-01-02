angular.module("libiada")
    .directive("tableParse", ["$parse", "$document",
        function ($parse, $document) {
            return {
                restrict: "E",
                link: function (scope, element, attrs) {
                    let inFocus = false;
                    function parseTabular(text) {
                        //The array we will return
                        let toReturn = [];
                        try {
                            //Pasted data split into rows
                            let rows = text.split(/[\n\f\r]/);
                            rows.forEach(thisRow => {
                                let row = thisRow.trim();
                                if (row != "") {
                                    let cols = row.split("\t");
                                    toReturn.push(cols);
                                }
                            });
                        }
                        catch (err) {
                            console.log("error parsing as tabular data");
                            console.log(err);
                            return null;
                        }
                        return toReturn;
                    }
                    function textChanged() {
                        let text = $("#myPasteBox").val();
                        if (text != "") {
                            //We need to change the scope values
                            scope.$apply(() => {
                                if (attrs.ngModel != undefined && attrs.ngModel != "") {
                                    $parse(attrs.ngModel).assign(scope, text);
                                }
                                if (attrs.ngArray != undefined && attrs.ngArray != "") {
                                    let asArray = parseTabular(text);
                                    if (asArray != null) {
                                        $parse(attrs.ngArray).assign(scope, asArray);
                                    }
                                }
                            });
                        }
                    }
                    $document.ready(() => {
                        //Handles the Ctrl + V keys for pasting
                        function handleKeyDown(e, args) {
                            if (!inFocus && e.which == keyCodes.V && (e.ctrlKey || e.metaKey)) { // CTRL + V
                                //reset value of our box
                                $("#myPasteBox").val("");
                                //set it in focus so that pasted text goes inside the box
                                $("#myPasteBox").focus();
                            }
                        }
                        //We add a text area to the body only if it is not already created by another myPaste directive
                        if ($("#myPasteBox").length == 0) {
                            $("body").append($('<textarea id=\"myPasteBox\" style=\"position:absolute; left:-1000px; top:-1000px;\"></textarea>'));
                            let keyCodes = {
                                'C': 67,
                                'V': 86
                            };

                            //If this is true, we wont respond to Ctrl + V
                            $("body").on("focus", "input, textarea", () => { inFocus = true; });

                            //We are not on a text element so we will respond
                            //to Ctrl + V
                            $("body").on("blur", "input, textarea", () => { inFocus = false; });

                            //Handle the key down event
                            $(document).keydown(handleKeyDown);

                            //We will respond to when the textbox value changes
                            $("#myPasteBox").bind("input propertychange", textChanged);
                        }
                        else {
                            //We will respond to when the textbox value changes
                            $("#myPasteBox").bind("input propertychange", textChanged);
                        }
                    });
                }
            }
        }]);
