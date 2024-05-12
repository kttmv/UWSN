import { create } from 'zustand'

interface State {
    consoleOutput: string[]
    isOpen: boolean
    addLineToConsoleOutput: (value: string, force?: boolean) => void
    clearConsoleOutput: () => void
    setIsOpen: (value: boolean) => void
}

const MAX_CACHE_SIZE = 1000
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
    clearConsoleOutput: () => {
        set(() => ({ consoleOutput: [] }))
    },
    setIsOpen: (value: boolean) => set({ isOpen: value })
}))

export default useConsoleStore
