import { create } from 'zustand'

export type SensorNodeData = {
    Id: number
    Position: {
        X: number
        Y: number
        Z: number
    }
}

interface State {
    sensorNodes: SensorNodeData[]
    addSensorNode: (sensor: SensorNodeData) => void
    clearSensorsNodes: () => void

    consoleOutput: string[]
    addLineToConsoleOutput: (value: string) => void
}

const useApplicationStore = create<State>((set) => ({
    consoleOutput: [],
    addLineToConsoleOutput: (value: string) =>
        set((state) => ({ consoleOutput: [...state.consoleOutput, value] })),
    sensorNodes: [],
    addSensorNode: (sensor) =>
        set((state) => ({ sensorNodes: [...state.sensorNodes, sensor] })),
    clearSensorsNodes: () => set(() => ({ sensorNodes: [] }))
}))

export default useApplicationStore
