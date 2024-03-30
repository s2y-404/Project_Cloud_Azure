// Path: api
const URL = "http://localhost:5290";

var osVM = null;
var nameVM = null;
var vmCountMax = 0;

var container = document.getElementById("group-inputs");
var textareaInfo = document.getElementById('textarea');

if (localStorage.getItem("info") === null) {
    localStorage.setItem("info", "> Connect you ;) ...");
}


var countIp = function () {
    var IPs = JSON.parse(localStorage.getItem("IPs"));
    return (IPs != null) ? IPs.length : 0;
}

// --------------------------------------------

// Statut de connexion
var changeStatus = function () {
    document.getElementById("statusText").innerText = localStorage.getItem("user") ? `Connecté en tant que: ${JSON.parse(localStorage.getItem("user")).username} | ${JSON.parse(localStorage.getItem("user")).role}` : "Non connecté";
    document.getElementById("statusText").style.color = localStorage.getItem("user") ? "green" : "red";

    document.getElementById("username").disabled = localStorage.getItem("user") ? true : false;
    document.getElementById("password").disabled = localStorage.getItem("user") ? true : false;

    document.getElementById("logoutButton").style.display = localStorage.getItem("user") ? "block" : "none";
    document.getElementById("loginButton").style.display = localStorage.getItem("user") ? "none" : "block";
}

// Gestion des droits d'accès par rapport au rôle
var gestionDroits = function () {
    if (localStorage.getItem("user") === null) return;

    const role = JSON.parse(localStorage.getItem("user")).role;

    switch (role) {
        case "gold":
            vmCountMax = 3;
            document.getElementById("windowsButton").disabled = false;
            document.getElementById("ubuntuButton").disabled = false;
            document.getElementById("debianButton").disabled = false;
            break;
        case "silver":
            vmCountMax = 1;
            document.getElementById("debianButton").disabled = false;
            break;
        case "bronze":
            vmCountMax = 0;
            break;
        default:
            break;
    }

    if (countIp() >= vmCountMax) {
        document.getElementById("windowsButton").disabled = true;
        document.getElementById("ubuntuButton").disabled = true;
        document.getElementById("debianButton").disabled = true;
    }
}

// Verification du status, des droits de connexion et reaffichage des VMs
var checkAndVerification = function () {
    changeStatus();
    gestionDroits();
}

checkAndVerification();

// --------------------------------------------

// Ajout, récupération et suppression d'IPs
var addIp = function (nameVM, ipVM) {
    var IPs = JSON.parse(localStorage.getItem("IPs"));
    if (IPs === null) {
        IPs = [{ "name": nameVM, "ip": ipVM }];
    } else {
        IPs.push({ "name": nameVM, "ip": ipVM });
    }
    localStorage.setItem("IPs", JSON.stringify(IPs));
}
var getIp = function (nameVM) {
    console.log('getIp |Nom de la VM:', nameVM);
    var IPs = JSON.parse(localStorage.getItem("IPs"));
    IPs.forEach((value, key) => {
        if (value.name === nameVM) {
            console.log({ "name": value.name, "ip": value.ip });
            return { "name": value.name, "ip": value.ip };
        }
    });
}
var removeIp = function (nameVM) {
    var IPs = JSON.parse(localStorage.getItem("IPs"));
    IPs.forEach((value, key) => {
        if (value.name === nameVM) {
            IPs.splice(key, 1);
        }
    });
    localStorage.setItem("IPs", JSON.stringify(IPs));
}

// Ajout d'informations
var addInfo = function (info1, info2, info3) {
    if (info1 != null) {
        $('#textarea').val(`${info1}\n-----\n\n${$('#textarea').val()}`);
    }
    if (info2 != null) {
        $('#textarea').val(`${info2}\n-----\n${$('#textarea').val()}`);
    }
    if (info3 != null) {
        $('#textarea').val(`${info3}`);
    }
    localStorage.setItem("info", $('#textarea').val());
}

addInfo(null, null, `${localStorage.getItem("info") || "\t..."}`)

var addVmCount = function () {
    vmCount = +localStorage.getItem("vmCount") || 0;
    if (vmCount > 0) {
        vmCount += 1;
        localStorage.setItem("vmCount", vmCount);
    }
}

var removeVmCount = function () {
    vmCount = +localStorage.getItem("vmCount") || 0;
    if (vmCount > 0) {
        vmCount -= 1;
        localStorage.setItem("vmCount", vmCount);
    }
}
// --------------------------------------------

// Suppression automatique des VMs
var deleteTimerVMs = function (nameVM) {
    setTimeout(() => {
        addInfo(null, `Virtual machine ${nameVM} will be deleted automatically in 5 minutes.`);

        setTimeout(() => {
            addInfo(null, `Virtual machine ${nameVM} deleted automatically.`);
            removeIp(nameVM);
            gestionDroits();
            getVMs();
        }, 5 * 60 * 1000);

    }, 5 * 60 * 1000);
}

// Création de VM
var createVM = function (nameVM, osVM) {
    document.getElementById("windowsButton").disabled = true;
    document.getElementById("ubuntuButton").disabled = true;
    document.getElementById("debianButton").disabled = true;

    if (countIp() >= vmCountMax) {
        addInfo("(?) Info: You have reached the maximum number of VMs...");
        return;
    }

    addInfo("Creation in progress...\n\tPlease wait... and do not reload the page...");
    $.ajax({
        url: `${URL}/vm/create/${nameVM}/${osVM}`,
        method: 'GET',
        success: function (data) {
            addInfo(null, data);

            setTimeout(() => {
                addInfo(null, `Virtual machine ${nameVM} created successfully.\n(Your VM will delete automatically after 10 minutes)`);
            }, 2 * 1000);

            $.ajax({
                url: `${URL}/vm/ip/${nameVM}`,
                method: 'GET',
                success: function (data) {
                    console.log('IP fetch |Adresse IP de la VM récupérée avec succès:', data);

                    addIp(nameVM, data);
                    gestionDroits();
                    getVMs();
                    deleteTimerVMs(nameVM);
                },
                error: function (xhr, status, error) {
                    console.error('Erreur lors de la récupération de l\'adresse IP de la VM:', error);
                    addInfo(null, "/!\\ Error: on the IP recovery of the virtual machine");
                }
            });
        },
        error: function (xhr, status, error) {
            console.error('Erreur lors de la création de la VM:', error);
            addInfo(null, "/!\\ Error: on the creation of the virtual machine");
        }
    });
}

// Suppression de VM
var deleteVM = function (nameVM) {
    if (countIp() < 1) {
        addInfo("(?) Info: No virtual machine found...");
        return;
    }

    var ipTemp = getIp(nameVM);
    removeIp(nameVM);

    addInfo(`Deletion in progress...\n\tPlease wait...`);
    $.ajax({
        url: `${URL}/rg/delete/${nameVM}`,
        method: 'GET',
        success: function (data) {
            addInfo(null, `Virtual machine ${nameVM} deleted successfully.`);

            gestionDroits();
            getVMs();

            window.location.reload();
        },
        error: function (xhr, status, error) {
            console.error('Erreur lors de la suppression de la VM:', error);
            addInfo(null, "/!\\ Error: on the removal of the virtual machine");

            addIp(ipTemp.name, ipTemp.ip);
            gestionDroits();
            getVMs();
        }
    });
}

// --------------------------------------------

// Sélection du système d'exploitation
var selectOS = function (osSelected) {
    osVM = osSelected;
}

// Connexion
document.getElementById("loginButton").addEventListener("click", function (event) {
    event.preventDefault();

    var username = document.getElementById("username").value;
    var password = document.getElementById("password").value;

    // Récupération des utilisateurs à partir du fichier JSON
    fetch('users.json')
        .then(response => response.json())
        .then(users => {
            var foundUser = users.find(user => user.username === username && user.password === password);

            if (foundUser) {
                localStorage.setItem("user", JSON.stringify({ "username": username, "role": foundUser.role }));
                document.getElementById("passwordHelp").innerText = "";

                if (foundUser.role === "bronze") {
                    addInfo(null, `> You don't have the right to create a virtual machine.`);
                } else {
                    addInfo(null, `> Create a virtual machine...`);
                }
            } else {
                document.getElementById("username").value = "";
                document.getElementById("password").value = "";

                document.getElementById("username").focus();

                document.getElementById("passwordHelp").innerText = "Nom d'utilisateur ou mot de passe incorrect.";

                if (localStorage.getItem("user")) {
                    localStorage.removeItem("user");
                }
            }

            checkAndVerification();
        })
        .catch(error => {
            console.error('Erreur lors de la récupération des utilisateurs:', error);
        });

});

// Deconnexion
document.getElementById("logoutButton").addEventListener("click", function () {
    // var vmCount = localStorage.getItem("vmCount") || 0;
    if (countIp() > 0) {
        addInfo(`> Delete your virtual machine(s) before disconnecting.`);
        return;
    }
    localStorage.clear();
    localStorage.setItem("info", "");
    document.getElementById("textarea").value = "";
    addInfo(`> Connect you...`);

    checkAndVerification();
    window.location.reload();
});

// --------------------------------------------

// Ajout d'une nouvelle IP
var addIpElement = function (name, ip) {
    var divCol = document.createElement("div");
    divCol.classList.add("col-md-6", "ipElement");
    divCol.id = name;

    var label = document.createElement("label");
    label.textContent = name;
    label.classList.add("form-label", "nameLabel");
    label.id = name;

    var inputGroup = document.createElement("div");
    inputGroup.classList.add("input-group", "mb-3");

    var input = document.createElement("input");
    input.setAttribute("type", "text");
    input.setAttribute("class", "form-control");
    input.setAttribute("placeholder", "l'adresse IP de votre machine");
    input.setAttribute("value", (osVM === "windows") ? `${ip}` : `azureadminuser@${ip}`);
    input.setAttribute("readonly", "");

    var buttonCopy = document.createElement("button");
    buttonCopy.classList.add("btn", "btn-outline-secondary");
    buttonCopy.setAttribute("type", "button");

    var svgCopy = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svgCopy.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    svgCopy.setAttribute("viewBox", "0 0 384 512");
    svgCopy.setAttribute("style", "height: 20px;");
    svgCopy.innerHTML = '<path d="M384 336H192c-8.8 0-16-7.2-16-16V64c0-8.8 7.2-16 16-16l140.1 0L400 115.9V320c0 8.8-7.2 16-16 16zM192 384H384c35.3 0 64-28.7 64-64V115.9c0-12.7-5.1-24.9-14.1-33.9L366.1 14.1c-9-9-21.2-14.1-33.9-14.1H192c-35.3 0-64 28.7-64 64V320c0 35.3 28.7 64 64 64zM64 128c-35.3 0-64 28.7-64 64V448c0 35.3 28.7 64 64 64H256c35.3 0 64-28.7 64-64V416H272v32c0 8.8-7.2 16-16 16H64c-8.8 0-16-7.2-16-16V192c0-8.8 7.2-16 16-16H96V128H64z"/>';

    buttonCopy.appendChild(svgCopy);

    buttonCopy.addEventListener("click", function () {
        input.select();
        document.execCommand("copy");
        addInfo("* Copied text : " + input.value);
    });

    var buttonDelete = document.createElement("button");
    buttonDelete.classList.add("btn", "btn-outline-danger");
    buttonDelete.setAttribute("type", "button");

    var svgDelete = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svgDelete.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    svgDelete.setAttribute("viewBox", "0 0 384 512");
    svgDelete.setAttribute("style", "height: 20px;");
    svgDelete.innerHTML = '<path fill="red" d="M342.6 150.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L192 210.7 86.6 105.4c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L146.7 256 41.4 361.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L192 301.3 297.4 406.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L237.3 256 342.6 150.6z"/>';

    buttonDelete.appendChild(svgDelete);

    buttonDelete.addEventListener("click", function () {
        console.log('Click suppression de la VM:', name);
        deleteVM(name);
        divCol.remove();
    });

    inputGroup.appendChild(input);
    inputGroup.appendChild(buttonCopy);
    inputGroup.appendChild(buttonDelete);

    divCol.appendChild(label);
    divCol.appendChild(inputGroup);

    container.appendChild(divCol);

    checkAndVerification();
};

// Ajout des vm déjà créées
var getVMs = function () {
    container.innerHTML = "";

    IPs = JSON.parse(localStorage.getItem("IPs"));
    if (IPs != null) {
        IPs.forEach((value, key) => {
            console.log('VM |Nom de la VM:', value.name, '|Adresse IP de la VM:', value.ip);
            addIpElement(value.name, value.ip);
        });
    }
}

getVMs();

// --------------------------------------------

// Modal de création de VM
document.addEventListener('DOMContentLoaded', function () {
    const vmForm = document.getElementById('vmForm');

    vmForm.addEventListener('submit', function (event) {
        event.preventDefault();

        vmName = document.getElementById('vmNameInput').value;
        document.getElementById('vmNameInput').value = "";

        createVM(vmName, osVM);
    });
});

