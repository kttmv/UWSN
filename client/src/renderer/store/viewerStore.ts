import { create } from 'zustand'
import { Sensor } from '../shared/types/sensor'

type State = {
    scale: number
    isOpen: boolean
    selectedSensor: Sensor | undefined

    setScale: (value: number) => void
    setIsOpen: (value: boolean) => void
    setSelectedSensor: (value: Sensor | undefined) => void
}

const useViewerStore = create<State>((set) => ({
    scale: 400,
    isOpen: false,
    selectedSensor: undefined,

    setScale: (value: number) => {
        set({ scale: value })
    },

    setIsOpen: (value: boolean) => {
        set({ isOpen: value })
    },

    setSelectedSensor: (value: Sensor | undefined) => {
        set({ selectedSensor: value })
    }
}))

export default useViewerStore
