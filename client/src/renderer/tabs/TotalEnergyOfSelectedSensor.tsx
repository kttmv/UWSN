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
import useViewerStore from '../store/viewerStore'

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
)

export default function TotalEnergyOfSelectedSensor() {
    const { selectedSensor } = useViewerStore()

    const { project } = useProjectStore()

    if (!project) throw new Error('Project не определен')
    if (!project.Result) throw new Error('Результаты симуляции не определены')
    if (!selectedSensor) return <></>

    const deltas = project.Result.Deltas

    const labels: string[] = []
    const dataEnergy: number[] = []

    for (let i = 0; i < deltas.length; i += Math.floor(deltas.length / 100)) {
        const state = calculateSimulationState(i, project)

        let timeString = state.Time.replace('T', ' ')
        timeString = timeString.substring(0, timeString.indexOf('.'))
        labels.push(timeString)

        dataEnergy.push(state.Sensors[selectedSensor.Id].Battery)
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
                            text: `Энергия сенсора #${selectedSensor.Id} за все время симуляции`
                        }
                    },
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    scales: {
                        energy: {
                            position: 'left'
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
                        }
                    ]
                }}
            />
        </Box>
    )
}
