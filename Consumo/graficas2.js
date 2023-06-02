
function getProducts() {
    const start = document.getElementById('start').value;
    const end = document.getElementById('end').value;
    const product = document.getElementById('opcionesProductos').value;
    const bodyPost = {
        startDate: start,
        endDate: end,
        product: product
    };

    google.charts.load('current', { 'packages': ['corechart', 'controls'] });
    google.charts.setOnLoadCallback(() => {
        fetch("http://localhost:83/Product/GetProduct", {
            headers: { "Content-Type": "application/json" },
            credentials: 'include',
            method: 'POST',
            body: JSON.stringify(bodyPost)
        })
            .then(response => {
                if (!response.ok) {
                    throw response;
                }
                return response.json();
            })
            .then(info => {
                cargarGraficaVentas(info);
                console.log(info);
                addDownloadButtonListener(info);
            })
            .catch(error => console.log(error));
    });
}

function nombreProductos() {
    var comboBox = document.getElementById("opcionesProductos");

    fetch("http://localhost:83/Product/GetNameProducts")
        .then(response => response.json())
        .then(data => {
            data.forEach(function (productName) {
                var nuevaOpcion = document.createElement("option");
                nuevaOpcion.text = productName;
                comboBox.appendChild(nuevaOpcion);
            });
        })
        .catch(error => {
            console.error("Error al obtener los nombres de los productos:", error);
        });
}

function cargarGraficaVentas(info) {
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Mes');
    data.addColumn('number', 'Cantidad');

    info.forEach(f => {
        data.addRow([f.month.toString(), f.amount]);
    });

    var dashboard = new google.visualization.Dashboard(
        document.getElementById('dashboard_div')
    );

    var rangeSlider = new google.visualization.ControlWrapper({
        'controlType': 'NumberRangeFilter',
        'containerId': 'filter_div',
        'options': {
            'filterColumnLabel': 'Cantidad'
        }
    });

    var pieChart = new google.visualization.ChartWrapper({
        'chartType': 'Bar',
        'containerId': 'chart_div',
        'options': {
            'width': 500,
            'height': 500,
            'title': 'Venta Producto',
            'pieSliceText': 'value',
            'legend': 'right'
        }
    });

    var table = new google.visualization.ChartWrapper({
        'chartType': 'Table',
        'containerId': 'table_div',
        'options': {
            'width': 200,
            'height': '100%'
        }
    });

    dashboard.bind(rangeSlider, pieChart);
    dashboard.bind(rangeSlider, table);
    dashboard.draw(data);
}

function addDownloadButtonListener(info) {
    const downloadButton = document.getElementById('downloadButton');
    downloadButton.addEventListener('click', () => {
        const csvData = convertToCSV(info);
        const csvBlob = new Blob([csvData], { type: 'text/csv;charset=utf-8;' });

        // Crear un enlace para la descarga del archivo
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(csvBlob);
        link.setAttribute('download', 'datos_ventas.csv');
        link.click();
    });
}

function convertToCSV(info) {
    let csv = 'Mes,Cantidad\n';
    info.forEach(f => {
        const row = `${f.month},${f.amount}\n`;
        csv += row;
    });
    return csv;
}
