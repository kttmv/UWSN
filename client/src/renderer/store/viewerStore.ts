import { create } from 'zustand'
import { Sensor } from '../shared/types/sensor'
import { Signal } from '../shared/types/signal'

type State = {
    scale: number
    isOpen: boolean
    isFullscreen: boolean
    selectedSensor: Sensor | undefined
    selectedSignal: Signal | undefined

    setScale: (value: number) => void
    setIsOpen: (value: boolean) => void
    setIsFullscreen: (value: boolean) => void
    setSelectedSensor: (value: Sensor | undefined) => void
    setSelectedSignal: (value: Signal | undefined) => void
}

const useViewerStore = create<State>((set, get) => ({
    scale: 400,
    isOpen: false,
    isFullscreen: false,
    selectedSensor: undefined,
    selectedSignal: undefined,

    setScale: (value: number) => {
        set({ scale: value })
    },

    setIsOpen: (value: boolean) => {
        if (!value && get().isFullscreen) {
            set({ isOpen: value, isFullscreen: false })
        } else {
            set({ isOpen: value })
        }
    },

    setIsFullscreen: (value: boolean) => {
        set({ isFullscreen: value })
    },

    setSelectedSensor: (value: Sensor | undefined) => {
        set({ selectedSensor: value, selectedSignal: undefined })
    },

    setSelectedSignal: (value: Signal | undefined) => {
        set({ selectedSignal: value, selectedSensor: undefined })
    }
}))

export default useViewerStore
