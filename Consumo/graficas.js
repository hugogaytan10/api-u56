function getSales() {
    const start = document.getElementById('start').value;
    const end = document.getElementById('end').value;
    const bodyPost = {
        startDate: start,
        endDate: end
    }
    // Load the Visualization API and the controls package.
    google.charts.load('current', {'packages':['corechart', 'controls', 'table']});
    google.charts.setOnLoadCallback(() => {
        fetch("http://localhost:83/Product/GetSales",
            {
                headers: { "Content-Type": "application/json" },
                credentials: 'include',
                method: 'POST',
                body: JSON.stringify(bodyPost)
            }
        )
            .then(response => {
                if (!response.ok) {
                    throw response;
                }
                return response.json();
            })
            .then(info => {
                cargarGraficaVentas(info);
                console.log(info)
            })
            .catch(error => console.log(error));
    });
}
function cargarGraficaVentas(info) {
    
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Mes');
    data.addColumn('number', 'Ventas');
    data.addColumn('number', 'Año');
  
    info.forEach(f => {
        var mes = cambiarMes(f.month);
        data.addRow([mes, f.sales, f.year]);
      });
      
  
    var dashboard = new google.visualization.Dashboard(
      document.getElementById('dashboard_div')
    );
  
    var rangeSlider = new google.visualization.ControlWrapper({
      'controlType': 'NumberRangeFilter',
      'containerId': 'filter_div',
      'options': {
        'filterColumnLabel': 'Año'
      }
    });
  
    var BarChart = new google.visualization.ChartWrapper({
      'chartType': 'BarChart',
      'containerId': 'chart_div',
      'options': {
        'width': '100%',
        'height': 300,
        'pieSliceText': 'value',
        'legend': 'right'
      }
    });
    var table = new google.visualization.ChartWrapper({
      'chartType': 'Table',
      'containerId': 'table_div',
      'options': {
        'width': '100%',
        'height': '100%'
      }
    });
    
    dashboard.bind(rangeSlider, BarChart);
    dashboard.bind(rangeSlider, table);
    dashboard.draw(data);

}

function cambiarMes(numeroMes) {
    var meses = [
      'Enero',
      'Febrero',
      'Marzo',
      'Abril',
      'Mayo',
      'Junio',
      'Julio',
      'Agosto',
      'Septiembre',
      'Octubre',
      'Noviembre',
      'Diciembre'
    ];
  
    return meses[numeroMes - 1];
  }

