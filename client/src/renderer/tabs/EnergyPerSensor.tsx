import { Box } from '@chakra-ui/react'
import {
    CategoryScale,
    Chart as ChartJS,
    Legend,
    LinearScale,
    LineElement,
    PointElement,
    Title,
    Tooltip
} from 'chart.js'
import 'chart.js/auto' // ADD THIS
import { Bar } from 'react-chartjs-2'
import { useProjectStore } from '../store/projectStore'

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
)

export default function Results() {
    const { project, simulationState } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    if (!project.Result) {
        throw new Error('Результаты симуляции не определены')
    }

    const labels: number[] = []
    const data: number[] = []

    for (const sensor of simulationState.Sensors) {
        labels.push(sensor.Id)
        data.push(sensor.Battery)
    }

    return (
        <Box height='300px'>
            <Bar
                id='energy-per-sensor-chart'
                options={{
                    responsive: true,

                    // ВАЖНО: без этого параметра не работает автоматическое
                    // изменение размера графика.
                    maintainAspectRatio: false,

                    plugins: {
                        legend: {
                            position: 'top' as const
                        },
                        title: {
                            display: true,
                            text: 'Энергия каждого сенсора на текущий момент симуляции'
                        }
                    },
                    scales: {
                        energy: {
                            max: project.SensorSettings.InitialSensorBattery
                        }
                    }
                }}
                data={{
                    labels,
                    datasets: [
                        {
                            label: 'Энергия, Дж',
                            data,
                            borderColor: 'cyan',
                            backgroundColor: 'cyan',
                            yAxisID: 'energy'
                        }
                    ]
                }}
            />
        </Box>
    )
}
