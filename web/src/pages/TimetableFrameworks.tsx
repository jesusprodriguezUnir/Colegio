import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Clock, 
  Plus, 
  Trash2, 
  Save, 
  RefreshCw, 
  Coffee, 
  Sun, 
  Moon,
  ChevronRight,
  Zap,
  Info
} from 'lucide-react'
import { timetableFrameworksApi } from '../services/api'
import { AcademicSession, type TimetableFramework, type BreakDefinition, type AcademicSessionType } from '../types'

export default function TimetableFrameworks() {
  const [frameworks, setFrameworks] = useState<TimetableFramework[]>([])
  const [selected, setSelected] = useState<TimetableFramework | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [generating, setGenerating] = useState(false)
  
  // Form State
  const [name, setName] = useState('')
  const [sessionType, setSessionType] = useState<AcademicSessionType>(AcademicSession.Standard)
  const [morningStart, setMorningStart] = useState('09:00')
  const [morningEnd, setMorningEnd] = useState('13:00')
  const [hasAfternoon, setHasAfternoon] = useState(false)
  const [afternoonStart, setAfternoonStart] = useState('15:00')
  const [afternoonEnd, setAfternoonEnd] = useState('17:00')
  const [sessionDuration, setSessionDuration] = useState(60)
  const [breaks, setBreaks] = useState<Partial<BreakDefinition>[]>([])

  useEffect(() => {
    loadFrameworks()
  }, [])

  const loadFrameworks = async () => {
    setLoading(true)
    try {
      const res = await timetableFrameworksApi.getAll()
      setFrameworks(res.data)
      if (res.data.length > 0 && !selected) {
        handleSelect(res.data[0])
      }
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleSelect = (f: TimetableFramework) => {
    setSelected(f)
    setName(f.name)
    setSessionType(f.sessionType)
    setMorningStart(f.morningStart.substring(0, 5))
    setMorningEnd(f.morningEnd.substring(0, 5))
    setHasAfternoon(f.hasAfternoon)
    setAfternoonStart(f.afternoonStart?.substring(0, 5) || '15:00')
    setAfternoonEnd(f.afternoonEnd?.substring(0, 5) || '17:00')
    setSessionDuration(f.sessionDurationMinutes)
    setBreaks(f.breaks.map(b => ({ ...b, startTime: b.startTime.substring(0, 5), endTime: b.endTime.substring(0, 5) })))
  }

  const handleCreateNew = () => {
    setSelected(null)
    setName('Nuevo Marco Temporal')
    setSessionType(AcademicSession.Standard)
    setMorningStart('09:00')
    setMorningEnd('14:00')
    setHasAfternoon(false)
    setSessionDuration(60)
    setBreaks([])
  }

  const handleAddBreak = () => {
    setBreaks([...breaks, { id: crypto.randomUUID(), label: 'Recreo', startTime: '11:00', endTime: '11:30' }])
  }

  const handleRemoveBreak = (id: string) => {
    setBreaks(breaks.filter(b => b.id !== id))
  }

  const handleSave = async () => {
    setSaving(true)
    const data = {
      name,
      sessionType,
      morningStart: morningStart + ':00',
      morningEnd: morningEnd + ':00',
      hasAfternoon,
      afternoonStart: hasAfternoon ? afternoonStart + ':00' : null,
      afternoonEnd: hasAfternoon ? afternoonEnd + ':00' : null,
      sessionDurationMinutes: sessionDuration,
      breaks: breaks.map(b => ({ ...b, startTime: b.startTime + ':00', endTime: b.endTime + ':00' }))
    }

    try {
      if (selected) {
        await timetableFrameworksApi.update(selected.id, data)
      } else {
        await timetableFrameworksApi.create(data)
      }
      loadFrameworks()
    } catch (e) {
      console.error(e)
    } finally {
      setSaving(false)
    }
  }

  const handleGenerate = async () => {
    if (!selected) return
    setGenerating(true)
    try {
      await timetableFrameworksApi.generateSlots(selected.id)
      alert('Bloques horarios generados con éxito')
    } catch (e) {
      console.error(e)
    } finally {
      setGenerating(false)
    }
  }

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="max-w-[1400px] mx-auto space-y-8 pb-20"
    >
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div>
          <h1 className="text-3xl font-display font-bold text-surface-900 tracking-tight flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-brand-600 flex items-center justify-center text-white">
              <Clock size={20} />
            </div>
            Marcos Temporales
          </h1>
          <p className="text-surface-500 font-medium mt-1">Configura la jornada escolar y genera los bloques horarios</p>
        </div>
        
        <button 
          onClick={handleCreateNew}
          className="btn-primary flex items-center gap-2"
        >
          <Plus size={20} />
          Nuevo Marco
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        {/* Sidebar: List of Frameworks */}
        <div className="lg:col-span-1 space-y-4">
          <div className="glass-card p-4 space-y-2">
            <h3 className="text-xs font-black text-surface-400 uppercase tracking-widest mb-4 px-2">Configuraciones</h3>
            {loading ? (
              <div className="space-y-2">
                {[1, 2].map(i => <div key={i} className="h-16 bg-surface-100 rounded-xl animate-pulse" />)}
              </div>
            ) : frameworks.length === 0 ? (
              <div className="p-8 text-center text-surface-400 italic text-sm">
                No hay marcos definidos
              </div>
            ) : frameworks.map(f => (
              <button
                key={f.id}
                onClick={() => handleSelect(f)}
                className={`w-full text-left p-4 rounded-xl transition-all flex items-center justify-between group ${
                  selected?.id === f.id 
                    ? 'bg-brand-600 text-white shadow-lg shadow-brand-200' 
                    : 'hover:bg-surface-50 text-surface-700'
                }`}
              >
                <div>
                  <p className="font-bold text-sm">{f.name}</p>
                  <p className={`text-[10px] font-bold uppercase tracking-wider mt-1 ${selected?.id === f.id ? 'text-brand-100' : 'text-surface-400'}`}>
                    {f.sessionType === AcademicSession.Standard ? 'Estándar' : 'Intensivo'}
                  </p>
                </div>
                <ChevronRight size={16} className={selected?.id === f.id ? 'text-white' : 'text-surface-300 group-hover:translate-x-1 transition-transform'} />
              </button>
            ))}
          </div>
          
          <div className="bg-amber-50 border border-amber-200 p-4 rounded-2xl flex gap-3">
            <Info size={20} className="text-amber-600 shrink-0 mt-0.5" />
            <p className="text-xs text-amber-800 leading-relaxed font-medium">
              Al generar los bloques horarios, se borrarán los anteriores para este marco. Las clases ya asignadas a slots que desaparezcan podrían perderse.
            </p>
          </div>
        </div>

        {/* Main Content: Editor */}
        <div className="lg:col-span-3 space-y-6">
          <div className="glass-card p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-xl font-bold text-surface-900">Configuración del Marco</h2>
              <div className="flex gap-3">
                {selected && (
                  <button 
                    onClick={handleGenerate}
                    disabled={generating}
                    className="btn-secondary flex items-center gap-2 text-brand-600 border-brand-100 hover:bg-brand-50"
                  >
                    <Zap size={18} className={generating ? 'animate-spin' : ''} />
                    {generating ? 'Generando...' : 'Generar Slots'}
                  </button>
                )}
                <button 
                  onClick={handleSave}
                  disabled={saving}
                  className="btn-primary flex items-center gap-2"
                >
                  <Save size={18} />
                  {saving ? 'Guardando...' : 'Guardar Cambios'}
                </button>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {/* General Info */}
              <div className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-bold text-surface-700">Nombre del Marco</label>
                  <input 
                    value={name}
                    onChange={e => setName(e.target.value)}
                    className="input-field w-full text-lg font-bold"
                    placeholder="Ej: Horario Invierno 2024"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-surface-700">Tipo de Sesión</label>
                    <select 
                      value={sessionType}
                      onChange={e => setSessionType(Number(e.target.value) as AcademicSessionType)}
                      className="input-field w-full"
                    >
                      <option value={AcademicSession.Standard}>Estándar (Oct-May)</option>
                      <option value={AcademicSession.Intensive}>Intensivo (Jun/Sep)</option>
                    </select>
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-surface-700">Duración Clase (min)</label>
                    <input 
                      type="number"
                      value={sessionDuration}
                      onChange={e => setSessionDuration(Number(e.target.value))}
                      className="input-field w-full"
                    />
                  </div>
                </div>

                <div className="h-px bg-surface-100 my-2" />

                {/* Morning Session */}
                <div className="space-y-4">
                  <div className="flex items-center gap-2 text-brand-600">
                    <Sun size={18} />
                    <h4 className="font-bold text-sm uppercase tracking-wider">Sesión de Mañana</h4>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <label className="text-xs font-bold text-surface-500 uppercase">Inicio</label>
                      <input 
                        type="time"
                        value={morningStart}
                        onChange={e => setMorningStart(e.target.value)}
                        className="input-field w-full"
                      />
                    </div>
                    <div className="space-y-2">
                      <label className="text-xs font-bold text-surface-500 uppercase">Fin</label>
                      <input 
                        type="time"
                        value={morningEnd}
                        onChange={e => setMorningEnd(e.target.value)}
                        className="input-field w-full"
                      />
                    </div>
                  </div>
                </div>

                {/* Afternoon Session */}
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 text-indigo-600">
                      <Moon size={18} />
                      <h4 className="font-bold text-sm uppercase tracking-wider">Sesión de Tarde</h4>
                    </div>
                    <label className="relative inline-flex items-center cursor-pointer">
                      <input type="checkbox" checked={hasAfternoon} onChange={e => setHasAfternoon(e.target.checked)} className="sr-only peer" />
                      <div className="w-11 h-6 bg-surface-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-surface-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-brand-600"></div>
                    </label>
                  </div>
                  <AnimatePresence>
                    {hasAfternoon && (
                      <motion.div 
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: 'auto', opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        className="grid grid-cols-2 gap-4 overflow-hidden"
                      >
                        <div className="space-y-2">
                          <label className="text-xs font-bold text-surface-500 uppercase">Inicio</label>
                          <input 
                            type="time"
                            value={afternoonStart}
                            onChange={e => setAfternoonStart(e.target.value)}
                            className="input-field w-full"
                          />
                        </div>
                        <div className="space-y-2">
                          <label className="text-xs font-bold text-surface-500 uppercase">Fin</label>
                          <input 
                            type="time"
                            value={afternoonEnd}
                            onChange={e => setAfternoonEnd(e.target.value)}
                            className="input-field w-full"
                          />
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </div>
              </div>

              {/* Breaks Section */}
              <div className="space-y-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-surface-900">
                    <Coffee size={18} />
                    <h4 className="font-bold text-sm uppercase tracking-wider">Recreos y Comidas</h4>
                  </div>
                  <button 
                    onClick={handleAddBreak}
                    className="text-brand-600 hover:text-brand-700 text-xs font-black uppercase tracking-widest flex items-center gap-1.5"
                  >
                    <Plus size={14} />
                    Añadir
                  </button>
                </div>

                <div className="space-y-3">
                  {breaks.map((b, i) => (
                    <div key={b.id || i} className="p-4 rounded-2xl bg-surface-50 border border-surface-100 space-y-4">
                      <div className="flex items-center justify-between">
                        <input 
                          value={b.label}
                          onChange={e => setBreaks(breaks.map(br => br.id === b.id ? { ...br, label: e.target.value } : br))}
                          className="bg-transparent font-bold text-surface-900 border-none p-0 focus:ring-0 w-full"
                        />
                        <button onClick={() => handleRemoveBreak(b.id!)} className="text-surface-400 hover:text-red-500 transition-colors">
                          <Trash2 size={16} />
                        </button>
                      </div>
                      <div className="grid grid-cols-2 gap-3">
                        <input 
                          type="time"
                          value={b.startTime}
                          onChange={e => setBreaks(breaks.map(br => br.id === b.id ? { ...br, startTime: e.target.value } : br))}
                          className="input-field py-1.5 text-xs"
                        />
                        <input 
                          type="time"
                          value={b.endTime}
                          onChange={e => setBreaks(breaks.map(br => br.id === b.id ? { ...br, endTime: e.target.value } : br))}
                          className="input-field py-1.5 text-xs"
                        />
                      </div>
                    </div>
                  ))}
                  {breaks.length === 0 && (
                    <div className="p-12 text-center border-2 border-dashed border-surface-100 rounded-3xl text-surface-400 text-sm font-medium">
                      No hay recreos definidos
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Timeline Preview */}
          <div className="glass-card p-8">
            <h3 className="text-lg font-bold text-surface-900 mb-6 flex items-center gap-2">
              <RefreshCw size={20} className="text-surface-400" />
              Vista Previa de la Jornada
            </h3>
            <div className="relative pt-8 pb-4">
               {/* Timeline labels */}
               <div className="flex justify-between text-[10px] font-black text-surface-400 uppercase tracking-widest border-b border-surface-100 pb-2">
                 <span>{morningStart}</span>
                 {hasAfternoon && <span>{afternoonEnd}</span>}
                 {!hasAfternoon && <span>{morningEnd}</span>}
               </div>
               
               <div className="mt-4 flex gap-1 h-12 bg-surface-50 rounded-xl overflow-hidden p-1">
                 {/* This is a simplified visual representation */}
                 <div className="flex-1 bg-brand-100 border border-brand-200 rounded-lg flex items-center justify-center">
                    <span className="text-[10px] font-bold text-brand-600 uppercase">Clases Mañana</span>
                 </div>
                 {breaks.map((b, i) => (
                   <div key={i} className="w-16 bg-amber-100 border border-amber-200 rounded-lg flex items-center justify-center" title={b.label}>
                     <Coffee size={14} className="text-amber-600" />
                   </div>
                 ))}
                 {hasAfternoon && (
                   <>
                     <div className="w-12 bg-surface-200/50" /> {/* Gap for lunch if exists */}
                     <div className="flex-[0.5] bg-indigo-100 border border-indigo-200 rounded-lg flex items-center justify-center">
                        <span className="text-[10px] font-bold text-indigo-600 uppercase">Tarde</span>
                     </div>
                   </>
                 )}
               </div>
               <p className="text-[11px] text-surface-400 italic mt-4">
                 * El generador creará sesiones de {sessionDuration} minutos consecutivas, saltando los recreos.
               </p>
            </div>
          </div>
        </div>
      </div>
    </motion.div>
  )
}
