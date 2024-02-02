import create from 'zustand'

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

    consoleOutput: string
    setConsoleOutput: (value: string) => void
}

const useAppStore = create<State>((set) => ({
    consoleOutput: '',
    setConsoleOutput: (value: string) => set(() => ({ consoleOutput: value })),
    sensorNodes: [],
    addSensorNode: (sensor) =>
        set((state) => ({ sensorNodes: [...state.sensorNodes, sensor] })),
    clearSensorsNodes: () => set(() => ({ sensorNodes: [] }))
}))

export default useAppStore
