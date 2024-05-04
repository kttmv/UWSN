import { create } from 'zustand'

type State = {
    scale: number
    isOpen: boolean

    setScale: (value: number) => void
    setIsOpen: (value: boolean) => void
}

const useViewerStore = create<State>((set) => ({
    scale: 100,
    isOpen: false,

    setScale: (value: number) => {
        set({ scale: value })
    },

    setIsOpen: (value: boolean) => {
        set({ isOpen: value })
    }
}))

export default useViewerStore
