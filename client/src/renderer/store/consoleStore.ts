import { create } from 'zustand'

interface State {
    consoleOutput: string[]
    isOpen: boolean
    addLineToConsoleOutput: (value: string, force?: boolean) => void
    setIsOpen: (value: boolean) => void
}

const MAX_CACHE_SIZE = 100
let cache: string[] = []

const useConsoleStore = create<State>((set) => ({
    consoleOutput: [],
    isOpen: false,
    addLineToConsoleOutput: (value: string, force?: boolean) => {
        cache.push(value)

        if (cache.length >= MAX_CACHE_SIZE || force) {
            set((state) => ({
                consoleOutput: [...state.consoleOutput, ...cache]
            }))
            cache = []
        }
    },
    setIsOpen: (value: boolean) => set({ isOpen: value })
}))

export default useConsoleStore
