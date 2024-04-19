import { create } from 'zustand'

type State = {
    scale: number
    setScale: (value: number) => void
}

const useViewerSettingsStore = create<State>((set) => ({
    scale: 100,
    setScale: (value: number) => {
        set(() => ({ scale: value }))
    }
}))

export default useViewerSettingsStore
