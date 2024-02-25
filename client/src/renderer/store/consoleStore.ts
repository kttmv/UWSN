import { create } from 'zustand'

interface State {
    consoleOutput: string[]
    addLineToConsoleOutput: (value: string) => void
}

const useConsoleStore = create<State>((set) => ({
    consoleOutput: [],
    addLineToConsoleOutput: (value: string) =>
        set((state) => ({ consoleOutput: [...state.consoleOutput, value] }))
}))

export default useConsoleStore
