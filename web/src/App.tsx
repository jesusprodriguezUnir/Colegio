import { Routes, Route } from 'react-router-dom'
import Sidebar from './components/Sidebar'
import Header from './components/Header'
import Dashboard from './pages/Dashboard'
import Schools from './pages/Schools'
import Teachers from './pages/Teachers'
import Students from './pages/Students'
import Classrooms from './pages/Classrooms'
import Invoices from './pages/Invoices'
import Administration from './pages/Administration'
import Schedules from './pages/Schedules'
import ClassUnits from './pages/ClassUnits'
import TimetableFrameworks from './pages/TimetableFrameworks'
import { AnimatePresence } from 'framer-motion'

function App() {
  return (
    <div className="flex min-h-screen bg-surface-50">
      <Sidebar />
      
      <div className="flex-1 flex flex-col min-w-0">
        <Header />
        
        <main className="flex-1 p-8">
          <AnimatePresence mode="wait">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/schools" element={<Schools />} />
              <Route path="/teachers" element={<Teachers />} />
              <Route path="/students" element={<Students />} />
              <Route path="/classrooms" element={<Classrooms />} />
              <Route path="/schedules" element={<Schedules />} />
              <Route path="/classunits" element={<ClassUnits />} />
              <Route path="/frameworks" element={<TimetableFrameworks />} />
              <Route path="/invoices" element={<Invoices />} />
              <Route path="/administration" element={<Administration />} />
            </Routes>
          </AnimatePresence>
        </main>
      </div>
    </div>
  )
}

export default App