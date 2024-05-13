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
import { Line } from 'react-chartjs-2'
import {
    calculateSimulationState,
    useProjectStore
} from '../store/projectStore'

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
    const { project } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    if (!project.Result) {
        throw new Error('Результаты симуляции не определены')
    }

    const deltas = project.Result.Deltas

    const labels: string[] = []
    const dataEnergy: number[] = []
    const dataDeadSensors: number[] = []

    for (let i = 0; i < deltas.length; i += Math.floor(deltas.length / 100)) {
        const state = calculateSimulationState(i, project)

        let timeString = state.Time.replace('T', ' ')
        timeString = timeString.substring(0, timeString.indexOf('.'))
        labels.push(timeString)

        let totalEnery = 0
        let deadSensors = 0
        for (const sensor of state.Sensors) {
            totalEnery += sensor.Battery
            if (sensor.Battery < project.SensorSettings.BatteryDeadCharge) {
                deadSensors++
            }
        }

        dataEnergy.push(totalEnery)
        dataDeadSensors.push(deadSensors)
    }

    return (
        <Box height='300px'>
            <Line
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
                            text: 'Энергетические показатели сети за все время симуляции'
                        }
                    },
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    scales: {
                        energy: {
                            position: 'left'
                        },
                        dead: {
                            position: 'right',
                            grid: {
                                drawOnChartArea: false
                            },
                            max: project.Environment.Sensors.length
                        }
                    }
                }}
                data={{
                    labels,
                    datasets: [
                        {
                            label: 'Энергия, Дж',
                            data: dataEnergy,
                            borderColor: 'cyan',
                            backgroundColor: 'cyan',
                            yAxisID: 'energy'
                        },
                        {
                            label: 'Мертвых сенсоров, шт.',
                            data: dataDeadSensors,
                            borderColor: 'red',
                            backgroundColor: 'red',
                            yAxisID: 'dead'
                        }
                    ]
                }}
            />
        </Box>
    )
}
