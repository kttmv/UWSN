//import Plot from 'react-plotly.js'
import {
    calculateSimulationState,
    useProjectStore
} from '../store/projectStore'

export default function Results() {
    const { project } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    if (!project.Result) {
        throw new Error('Результаты симуляции не определены')
    }

    const deltas = project.Result.Deltas

    type plotlyData = {
        x: string[]
        y: number[]
    }

    const data: plotlyData = {
        x: [],
        y: []
    }

    for (let i = 0; i < deltas.length; i += Math.ceil(deltas.length / 100)) {
        const state = calculateSimulationState(i, project)
        let totalEnery = 0
        for (const sensor of state.Sensors) {
            totalEnery += sensor.Battery
        }
        data.x.push(state.Time.replace('T', ''))
        data.y.push(totalEnery)
    }

    //return <Plot />

    return <></>
}
