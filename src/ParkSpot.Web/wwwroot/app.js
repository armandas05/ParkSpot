const getElement = id => document.getElementById(id);
const isManagerPage = document.body.dataset.kind === "manager";
const pageSize = 30;

let currentUser;
let spotPage = 1;
let reservationPage = 1;
let selectedSpot;

function showMessage(text, isError = false) {
    const message = getElement("message");
    message.textContent = text;
    message.className = isError ? "error" : "";
    message.hidden = false;
}

function escapeHtml(value) {
    const replacements = {
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    };

    return String(value).replace(/[&<>"']/g, character => replacements[character]);
}

function readForm(form) {
    return Object.fromEntries(new FormData(form));
}

function parseUtcDate(value) {
    return new Date(value.endsWith("Z") ? value : value + "Z");
}

function formatDate(value) {
    return parseUtcDate(value).toLocaleString("lt-LT", {
        timeZone: "Europe/Vilnius",
        dateStyle: "short",
        timeStyle: "short"
    });
}

async function requestApi(path, method = "GET", body) {
    const headers = { "Content-Type": "application/json" };

    const response = await fetch("/api" + path, {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body)
    });

    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        const statusMessages = {
            401: "Prisijunkite iš naujo.",
            403: "Šis veiksmas jums neleidžiamas.",
        };

        throw new Error(
            error.message || statusMessages[response.status] || "Užklausa nepavyko."
        );
    }

    return response.status === 204 ? null : response.json();
}

function handleAction(handler) {
    return async event => {
        event?.preventDefault();
        const button = event?.submitter;
        if (button) button.disabled = true;

        try {
            await handler(event);
        } catch (error) {
            showMessage(error.message, true);
        } finally {
            if (button) button.disabled = false;
        }
    };
}

function configureManagerPage() {
    if (!isManagerPage) return;

    getElement("auth-title").textContent = "Administratoriaus prisijungimas";
    getElement("auth").querySelector(".muted").textContent =
        "Prisijunk arba užregistruok savo organizaciją.";
    getElement("switch-role").href = "/";
    getElement("switch-role").textContent = "Vairuotojui ↗";
}

async function loadSession() {
    try {
        currentUser = await requestApi("/auth/me");
    } catch {
        currentUser = null;
    }

    if (currentUser && (currentUser.role === "Manager") !== isManagerPage) {
        location.href = currentUser.role === "Manager" ? "/manager.html" : "/";
        return;
    }

    getElement("switch-role").hidden = !!currentUser;
    getElement("auth").hidden = !!currentUser;
    getElement("workspace").hidden = !currentUser;
    getElement("logout").hidden = !currentUser;
    getElement("identity").textContent = currentUser?.name || "";

    if (!currentUser) return;

    getElement("management").hidden = !isManagerPage;
    getElement("reservations-title").textContent =
        isManagerPage ? "Organizacijos rezervacijos" : "Mano rezervacijos";

    await loadLots();
    await loadReservations();
}

async function loadLots(selectedLotId) {
    const lots = await requestApi("/lots");
    const lotSelect = getElement("lot");
    lotSelect.replaceChildren(
        ...lots.map(lot => new Option(`${lot.name} · ${lot.address}`, lot.id))
    );

    if (selectedLotId) lotSelect.value = selectedLotId;
    getElement("spots-form").querySelector("button").disabled = !lots.length;

    if (lots.length) {
        await loadSpots();
    } else {
        getElement("spots").textContent = "Aikštelių dar nėra.";
        getElement("spot-count").textContent = "";
    }
}

function updatePagination(currentPage, total, labelId, previousId, nextId) {
    const totalPages = Math.max(1, Math.ceil(total / pageSize));
    getElement(labelId).textContent = `${currentPage} / ${totalPages}`;
    getElement(previousId).disabled = currentPage <= 1;
    getElement(nextId).disabled = currentPage * pageSize >= total;
}

async function loadSpots() {
    const lotId = getElement("lot").value;
    if (!lotId) return;

    const parameters = new URLSearchParams({
        page: spotPage,
        onlyAvailable: !isManagerPage
    });
    const result = await requestApi(`/lots/${lotId}/spots?${parameters}`);

    getElement("spot-count").textContent = `Rasta vietų: ${result.total}`;
    updatePagination(spotPage, result.total, "page-label", "previous", "next");
    getElement("spots").replaceChildren(...result.items.map(createSpotCard));

    if (!result.items.length) {
        getElement("spots").textContent = "Pagal pasirinktus filtrus vietų nerasta.";
    }
}

function createSpotCard(spot) {
    const card = document.createElement("article");
    const statusClass = !spot.isReservable ? " closed" : !spot.isAvailable ? " busy" : "";
    const statusText = !spot.isReservable ? "Uždaryta" : spot.isAvailable ? "Laisva" : "Užimta";

    card.className = "spot" + statusClass;
    card.innerHTML = `
        <strong>${escapeHtml(spot.number)}</strong>
        <small>${statusText}</small>
    `;

    const button = document.createElement("button");
    button.textContent = isManagerPage
        ? (spot.isReservable ? "Uždaryti" : "Atverti")
        : "Rezervuoti";
    button.disabled = !isManagerPage && !spot.isAvailable;
    button.onclick = handleAction(() => selectSpot(spot));
    card.append(button);

    return card;
}

async function selectSpot(spot) {
    if (!isManagerPage) {
        openReservationDialog(spot);
        return;
    }

    await requestApi(`/manager/spots/${spot.id}`, "PATCH", {
        isReservable: !spot.isReservable
    });
    showMessage("Vietos prieinamumas atnaujintas. Esamos rezervacijos lieka galioti.");
    await loadSpots();
    await loadReservations();
}

function openReservationDialog(spot) {
    selectedSpot = spot;
    getElement("reservation-dialog-title").textContent = "Rezervuoti vietą " + spot.number;
    getElement("reservation-error").hidden = true;
    getElement("duration-hours").value = 1;
    getElement("reservation-dialog").showModal();
}

async function loadReservations() {
    const result = await requestApi("/reservations?page=" + reservationPage);
    updatePagination(
        reservationPage,
        result.total,
        "reservations-page",
        "reservations-previous",
        "reservations-next"
    );
    getElement("reservations").replaceChildren(...result.items.map(createReservationRow));

    if (!result.items.length) {
        getElement("reservations").textContent = "Rezervacijų dar nėra.";
    }
}

function createReservationRow(reservation) {
    const hasEnded = parseUtcDate(reservation.endTime).getTime() <= Date.now();
    const status = reservation.cancelled ? "Atšaukta" : hasEnded ? "Pasibaigusi" : "Aktyvi";
    const reservationId = isManagerPage
        ? `<code>${escapeHtml(reservation.reservationId)}</code>`
        : "";

    const row = document.createElement("article");
    row.className = "reservation";
    row.innerHTML = `
        <div>
            <strong>${escapeHtml(reservation.lotName)} · ${escapeHtml(reservation.spotNumber)}</strong>
            <p>${formatDate(reservation.startTime)} – ${formatDate(reservation.endTime)}</p>
            ${reservationId}
        </div>
        <aside>${status}</span></aside>
    `;

    if (!reservation.cancelled && !hasEnded) {
        const button = document.createElement("button");
        button.className = "secondary";
        button.textContent = "Atšaukti";
        button.onclick = handleAction(() => cancelReservation(reservation.reservationId));
        row.querySelector("aside").append(button);
    }

    return row;
}

async function cancelReservation(reservationId) {
    await requestApi(`/reservations/${reservationId}/cancel`, "POST");
    showMessage("Rezervacija atšaukta.");
    await loadReservations();
    await loadSpots();
}

async function login(event) {
    const role = isManagerPage ? "manager" : "driver";
    await requestApi(`/auth/login/${role}`, "POST", readForm(event.target));
    event.target.reset();
    await loadSession();
    showMessage("Sėkmingai prisijungėte.");
}

async function register(event) {
    const endpoint = isManagerPage ? "/auth/register/manager" : "/auth/register";
    await requestApi(endpoint, "POST", readForm(event.target));
    event.target.reset();
    getElement("register").open = false;
    showMessage("Paskyra sukurta. Dabar prisijunkite.");
}

async function logout() {
    await requestApi("/auth/logout", "POST");
    currentUser = null;
    await loadSession();
    showMessage("Atsijungėte.");
}

async function searchSpots() {
    spotPage = 1;
    await loadSpots();
}

async function createLot(event) {
    const lot = await requestApi("/manager/lots", "POST", readForm(event.target));
    event.target.reset();
    spotPage = 1;
    await loadLots(lot.id);
    showMessage("Aikštelė sukurta. Pridėkite parkavimo vietas.");
}

async function addSpots(event) {
    const form = readForm(event.target);
    await requestApi(`/manager/lots/${getElement("lot").value}/spots`, "POST", {
        count: Number(form.count)
    });
    await loadSpots();
    showMessage("Parkavimo vietos pridėtos.");
}

async function submitReservation(event) {
    event.preventDefault();
    const button = event.submitter;
    button.disabled = true;
    getElement("reservation-error").hidden = true;

    try {
        const reservation = await requestApi("/reservations/now", "POST", {
            parkingSpotId: selectedSpot.id,
            durationMinutes: Math.round(Number(getElement("duration-hours").value) * 60)
        });

        getElement("reservation-dialog").close();
        showMessage(
            `Vieta ${selectedSpot.number} rezervuota. ${formatDate(reservation.startTime)} – ${formatDate(reservation.endTime)}.`
        );
        await loadSpots();
        await loadReservations();
    } catch (error) {
        getElement("reservation-error").textContent = error.message;
        getElement("reservation-error").hidden = false;
    } finally {
        button.disabled = false;
    }
}

function bindPaginationEvents() {
    getElement("previous").onclick = handleAction(async () => {
        spotPage--;
        await loadSpots();
    });
    getElement("next").onclick = handleAction(async () => {
        spotPage++;
        await loadSpots();
    });
    getElement("reservations-previous").onclick = handleAction(async () => {
        reservationPage--;
        await loadReservations();
    });
    getElement("reservations-next").onclick = handleAction(async () => {
        reservationPage++;
        await loadReservations();
    });
}

function bindEvents() {
    getElement("login-form").onsubmit = handleAction(login);
    getElement("register-form").onsubmit = handleAction(register);
    getElement("logout").onclick = handleAction(logout);
    getElement("search-form").onsubmit = handleAction(searchSpots);
    getElement("lot").onchange = handleAction(searchSpots);
    getElement("refresh").onclick = handleAction(loadReservations);
    getElement("lot-form").onsubmit = handleAction(createLot);
    getElement("spots-form").onsubmit = handleAction(addSpots);
    getElement("reservation-form").onsubmit = submitReservation;
    getElement("close-reservation").onclick = () => getElement("reservation-dialog").close();

    bindPaginationEvents();
}

async function initialize() {
    configureManagerPage();
    bindEvents();
    await loadSession();
}

initialize().catch(error => showMessage(error.message, true));
