import { useState, useEffect } from 'react'
import { motion } from 'framer-motion'
import { Check, X, AlertTriangle, Heart, Save, RefreshCw } from 'lucide-react'
import { teachersApi } from '../services/api'
import { type TimeSlot, Days } from '../types'

interface Props {
  teacherId: string
  timeSlots: TimeSlot[]
  onSave: () => void
}

const LEVELS = [
  { value: 0, label: 'Preferido', icon: Heart, color: 'bg-rose-50 text-rose-600 border-rose-200 ring-rose-500', bg: 'bg-rose-500' },
  { value: 1, label: 'Disponible', icon: Check, color: 'bg-emerald-50 text-emerald-600 border-emerald-200 ring-emerald-500', bg: 'bg-emerald-500' },
  { value: 2, label: 'No Deseado', icon: AlertTriangle, color: 'bg-amber-50 text-amber-600 border-amber-200 ring-amber-500', bg: 'bg-amber-500' },
  { value: 3, label: 'No Disponible', icon: X, color: 'bg-slate-50 text-slate-400 border-slate-200 ring-slate-500', bg: 'bg-slate-500' },
]

export default function TeacherAvailabilityGrid({ teacherId, timeSlots, onSave }: Props) {
  const [grid, setGrid] = useState<Record<string, number>>({})
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  const standardSlots = timeSlots.filter(ts => ts.sessionType === 0 && !ts.isBreak)
  const uniqueTimeRanges = Array.from(new Set(standardSlots.map(ts => ts.startTime))).sort()
  const days = [Days.Monday, Days.Tuesday, Days.Wednesday, Days.Thursday, Days.Friday]
  const dayLabels = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes']

  useEffect(() => {
    loadAvailability()
  }, [teacherId])

  const loadAvailability = async () => {
    setLoading(true)
    try {
      const res = await teachersApi.getById(teacherId)
      const avails = res.data.availabilities || []
      const newGrid: Record<string, number> = {}
      avails.forEach((a: any) => {
        newGrid[a.timeSlotId] = a.level
      })
      setGrid(newGrid)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const cycleLevel = (slotId: string) => {
    setGrid(prev => {
      const current = prev[slotId] ?? 1 // Default to Available
      const next = (current + 1) % 4
      return { ...prev, [slotId]: next }
    })
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      const data = Object.entries(grid).map(([slotId, level]) => ({
        timeSlotId: slotId,
        level: level
      }))
      await teachersApi.updateAvailability(teacherId, data)
      onSave()
    } catch (e) {
      console.error(e)
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <div className="p-20 text-center"><RefreshCw className="animate-spin mx-auto text-brand-600" size={32} /></div>

  return (
    <div className="space-y-8">
      {/* Legend */}
      <div className="flex flex-wrap gap-4 justify-center bg-surface-50 p-6 rounded-[2rem] border border-surface-100 shadow-inner">
        {LEVELS.map(l => (
          <div key={l.value} className="flex items-center gap-3 px-4 py-2 bg-white rounded-2xl border border-surface-200 shadow-sm">
            <div className={`w-3 h-3 rounded-full ${l.bg}`} />
            <span className="text-xs font-black text-surface-700 uppercase tracking-widest">{l.label}</span>
          </div>
        ))}
      </div>

      {/* Grid */}
      <div className="overflow-x-auto rounded-[2.5rem] border border-surface-200 bg-white shadow-xl">
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-surface-50/50 border-b border-surface-200">
              <th className="p-6 text-left text-[10px] font-black text-surface-400 uppercase tracking-[0.2em] w-32 border-r border-surface-100 sticky left-0 z-10 bg-surface-50/50 backdrop-blur-md">Hora</th>
              {dayLabels.map(day => (
                <th key={day} className="p-6 text-center text-[10px] font-black text-surface-400 uppercase tracking-[0.2em]">{day}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {uniqueTimeRanges.map(time => (
              <tr key={time} className="border-b border-surface-50 last:border-0">
                <td className="p-5 bg-surface-50/30 border-r border-surface-100 sticky left-0 z-10 backdrop-blur-md">
                   <span className="text-brand-600 font-black text-base">{time.substring(0, 5)}</span>
                </td>
                {days.map(day => {
                  const slot = standardSlots.find(ts => ts.startTime === time && ts.dayOfWeek === day)
                  if (!slot) return <td key={day} className="p-2 bg-surface-50/10"></td>
                  
                  const level = grid[slot.id] ?? 1
                  const levelCfg = LEVELS.find(l => l.value === level)!
                  const Icon = levelCfg.icon

                  return (
                    <td key={day} className="p-2">
                      <motion.button
                        whileHover={{ scale: 1.05 }}
                        whileTap={{ scale: 0.95 }}
                        onClick={() => cycleLevel(slot.id)}
                        className={`w-full h-16 rounded-2xl border-2 transition-all flex flex-col items-center justify-center gap-1 group relative overflow-hidden ${levelCfg.color} hover:shadow-lg hover:shadow-brand-500/10`}
                      >
                        <Icon size={18} strokeWidth={3} className="transition-transform group-hover:scale-110" />
                        <span className="text-[9px] font-black uppercase tracking-widest opacity-80">{levelCfg.label}</span>
                        {/* Interactive glow effect */}
                        <div className={`absolute inset-0 opacity-0 group-hover:opacity-10 transition-opacity ${levelCfg.bg}`} />
                      </motion.button>
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex justify-end pt-4">
        <button 
          onClick={handleSave}
          disabled={saving}
          className="flex items-center gap-3 px-10 py-4 bg-brand-600 text-white rounded-[2rem] font-black uppercase tracking-widest hover:bg-brand-700 transition-all shadow-xl shadow-brand-500/20 disabled:opacity-50"
        >
          {saving ? <RefreshCw className="animate-spin" size={20} /> : <Save size={20} />}
          Guardar Configuración
        </button>
      </div>
    </div>
  )
}
