import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  BookOpen, 
  GraduationCap,
  ChevronRight,
  Clock,
  Settings2,
  LayoutGrid,
  Info,
  X,
  UserSquare2,
  Phone,
  Mail,
  User
} from 'lucide-react'
import { teachersApi, timeSlotsApi } from '../services/api'
import type { Teacher, TimeSlot } from '../types'
import TeacherAvailabilityGrid from '../components/TeacherAvailabilityGrid'

export default function Teachers() {
  const [teachers, setTeachers] = useState<Teacher[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [showAvailability, setShowAvailability] = useState(false)
  const [selectedTeacher, setSelectedTeacher] = useState<Teacher | null>(null)
  const [editing, setEditing] = useState<Teacher | null>(null)
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([])
  
  const [form, setForm] = useState({ 
    firstName: '', 
    lastName: '', 
    specialty: '', 
    email: '', 
    phone: '', 
    iban: '', 
    dateOfBirth: '', 
    hireDate: '',
    maxWorkingHours: 25,
    maxGapsPerDay: 1,
    minDailyHours: 2,
    preferCompactSchedule: true,
    preferredFreeDay: undefined as number | undefined
  })
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadTeachers()
    loadTimeSlots()
  }, [])

  const loadTeachers = async () => {
    setLoading(true)
    try {
      const res = await teachersApi.getAll()
      setTeachers(res.data)
      if (selectedTeacher) {
        const updated = res.data.find((t: Teacher) => t.id === selectedTeacher.id)
        if (updated) setSelectedTeacher(updated)
      }
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const loadTimeSlots = async () => {
    try {
      const res = await timeSlotsApi.getAll()
      setTimeSlots(res.data)
    } catch (e) {
      console.error(e)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editing) {
        await teachersApi.update(editing.id, form)
      } else {
        await teachersApi.create(form)
      }
      resetForm()
      loadTeachers()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ 
      firstName: '', 
      lastName: '', 
      specialty: '', 
      email: '', 
      phone: '', 
      iban: '', 
      dateOfBirth: '', 
      hireDate: '',
      maxWorkingHours: 25,
      maxGapsPerDay: 1,
      minDailyHours: 2,
      preferCompactSchedule: true,
      preferredFreeDay: undefined
    })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Deseas eliminar permanentemente a este profesor?')) {
      try {
        await teachersApi.delete(id)
        loadTeachers()
        if (selectedTeacher?.id === id) setSelectedTeacher(null)
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (teacher: Teacher) => {
    setEditing(teacher)
    setForm({ 
      firstName: teacher.firstName, 
      lastName: teacher.lastName, 
      specialty: teacher.specialty,
      email: teacher.email,
      phone: teacher.phone,
      iban: teacher.iban,
      dateOfBirth: teacher.dateOfBirth?.split('T')[0] || '',
      hireDate: teacher.hireDate?.split('T')[0] || '',
      maxWorkingHours: teacher.maxWorkingHours,
      maxGapsPerDay: teacher.maxGapsPerDay,
      minDailyHours: teacher.minDailyHours,
      preferCompactSchedule: teacher.preferCompactSchedule,
      preferredFreeDay: teacher.preferredFreeDay
    })
    setShowForm(true)
  }

  const formatGrade = (grade: number) => {
    if (grade >= 1 && grade <= 3) return `${grade + 2} Años`;
    if (grade >= 4 && grade <= 9) return `${grade - 3}º Prim.`;
    if (grade >= 10 && grade <= 13) return `${grade - 9}º ESO`;
    if (grade >= 14 && grade <= 15) return `${grade - 13}º Bach.`;
    return grade.toString();
  };

  const filteredTeachers = teachers.filter(t => 
    (t.firstName + ' ' + t.lastName).toLowerCase().includes(searchTerm.toLowerCase()) ||
    t.specialty.toLowerCase().includes(searchTerm.toLowerCase()) ||
    t.email.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      {/* Header Actions */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={18} />
          <input 
            type="text" 
            placeholder="Buscar por nombre, email o especialidad..." 
            className="input-field w-full pl-10"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <button 
          onClick={() => { setShowForm(true); setEditing(null); }} 
          className="btn-primary flex items-center gap-2 w-full sm:w-auto justify-center"
        >
          <Plus size={20} />
          Añadir Profesor
        </button>
      </div>

      {/* Main Content: Table and Detail Panel */}
      <div className="flex gap-6 items-start">
        <div className={`glass-card overflow-hidden transition-all duration-300 ${selectedTeacher ? 'flex-[2]' : 'w-full'}`}>
          {loading ? (
            <div className="p-12 text-center">
              <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
              <p className="text-surface-500 font-medium">Cargando claustro...</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-surface-50 border-b border-surface-200">
                    <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Docente</th>
                    <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Especialidad</th>
                    <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Tutoría</th>
                    <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-100">
                  <AnimatePresence>
                    {filteredTeachers.map((teacher) => (
                      <motion.tr 
                        key={teacher.id}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        onClick={() => setSelectedTeacher(teacher)}
                        className={`cursor-pointer transition-colors group ${selectedTeacher?.id === teacher.id ? 'bg-brand-50/50' : 'hover:bg-surface-50/50'}`}
                      >
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-600 font-bold overflow-hidden border border-indigo-100">
                              <img src={`https://ui-avatars.com/api/?name=${teacher.firstName}+${teacher.lastName}&background=6366f1&color=fff`} alt="" />
                            </div>
                            <div className="flex flex-col">
                              <span className="font-semibold text-surface-900 leading-tight">{teacher.firstName} {teacher.lastName}</span>
                              <span className="text-xs text-surface-400">{teacher.email}</span>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-blue-50 text-blue-700 border border-blue-100">
                            <BookOpen size={10} />
                            {teacher.specialty}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          {teacher.tutorOf ? (
                            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-100">
                              <GraduationCap size={10} />
                              {formatGrade(teacher.tutorOf.gradeLevel)} {teacher.tutorOf.line}
                            </span>
                          ) : (
                            <span className="text-xs text-surface-400 italic">No asignada</span>
                          )}
                        </td>
                        <td className="px-6 py-4 text-right">
                          <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button 
                              onClick={(e) => { e.stopPropagation(); handleEdit(teacher); }}
                              className="p-1.5 hover:bg-white hover:shadow-sm text-brand-600 rounded-lg transition-all"
                            >
                              <Edit2 size={14} />
                            </button>
                            <button 
                              onClick={(e) => { e.stopPropagation(); handleDelete(teacher.id); }}
                              className="p-1.5 hover:bg-white hover:shadow-sm text-red-600 rounded-lg transition-all"
                            >
                              <Trash2 size={14} />
                            </button>
                            <ChevronRight size={16} className="text-surface-300 ml-1" />
                          </div>
                        </td>
                      </motion.tr>
                    ))}
                  </AnimatePresence>
                  {!loading && filteredTeachers.length === 0 && (
                    <tr>
                      <td colSpan={4} className="px-6 py-12 text-center">
                        <UserSquare2 size={40} className="mx-auto text-surface-200 mb-3" />
                        <p className="text-surface-400">No se encontraron profesores que coincidan con la búsqueda.</p>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Teacher Detail Panel */}
        <AnimatePresence>
          {selectedTeacher && (
            <motion.div 
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 20 }}
              className="flex-1 glass-card p-0 overflow-hidden sticky top-6 max-h-[calc(100vh-160px)] overflow-y-auto"
            >
              <div className="p-6 bg-gradient-to-br from-brand-600 to-indigo-700 text-white relative">
                <button 
                  onClick={() => setSelectedTeacher(null)}
                  className="absolute top-4 right-4 p-1.5 bg-white/10 hover:bg-white/20 rounded-full transition-colors"
                >
                  <X size={18} />
                </button>
                <div className="flex flex-col items-center text-center mt-4">
                  <div className="w-20 h-20 rounded-full border-4 border-white/20 overflow-hidden mb-4 shadow-xl">
                    <img src={`https://ui-avatars.com/api/?name=${selectedTeacher.firstName}+${selectedTeacher.lastName}&background=fff&color=6366f1&size=128`} alt="" />
                  </div>
                  <h3 className="text-xl font-bold">{selectedTeacher.firstName} {selectedTeacher.lastName}</h3>
                  <p className="text-brand-100 text-sm font-medium">{selectedTeacher.specialty}</p>
                </div>
              </div>
              
              <div className="p-6 space-y-6">
                <div className="flex flex-col gap-2">
                   <button 
                    onClick={() => setShowAvailability(true)}
                    className="w-full flex items-center justify-center gap-2 py-3 bg-brand-50 text-brand-600 border border-brand-100 rounded-xl font-bold hover:bg-brand-100 transition-all shadow-sm"
                  >
                    <LayoutGrid size={18} />
                    Configurar Disponibilidad
                  </button>
                </div>

                <div className="space-y-4">
                  <h4 className="text-xs font-bold text-surface-400 uppercase tracking-widest">Información de Contacto</h4>
                  <div className="space-y-3">
                    <div className="flex items-center gap-3 text-sm text-surface-600">
                      <div className="w-8 h-8 rounded-lg bg-surface-50 flex items-center justify-center text-surface-400">
                        <Mail size={16} />
                      </div>
                      <span>{selectedTeacher.email}</span>
                    </div>
                    <div className="flex items-center gap-3 text-sm text-surface-600">
                      <div className="w-8 h-8 rounded-lg bg-surface-50 flex items-center justify-center text-surface-400">
                        <Phone size={16} />
                      </div>
                      <span>{selectedTeacher.phone}</span>
                    </div>
                  </div>
                </div>

                <div className="space-y-4 pt-2 border-t border-surface-100">
                  <h4 className="text-xs font-bold text-surface-400 uppercase tracking-widest">Preferencias de Horario</h4>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="p-3 bg-surface-50 rounded-xl">
                      <span className="text-[10px] text-surface-400 uppercase font-black block mb-1">Huecos Máx/Día</span>
                      <span className="text-sm font-bold text-surface-700">{selectedTeacher.maxGapsPerDay}</span>
                    </div>
                    <div className="p-3 bg-surface-50 rounded-xl">
                      <span className="text-[10px] text-surface-400 uppercase font-black block mb-1">Mín. Horas/Día</span>
                      <span className="text-sm font-bold text-surface-700">{selectedTeacher.minDailyHours}h</span>
                    </div>
                    <div className="p-3 bg-surface-50 rounded-xl col-span-2 flex items-center justify-between">
                      <span className="text-[10px] text-surface-400 uppercase font-black">Horario Compacto</span>
                      <span className={`px-2 py-0.5 rounded-lg text-[10px] font-bold ${selectedTeacher.preferCompactSchedule ? 'bg-green-100 text-green-700' : 'bg-surface-200 text-surface-500'}`}>
                        {selectedTeacher.preferCompactSchedule ? 'SÍ' : 'NO'}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="space-y-4 pt-2 border-t border-surface-100">
                  <h4 className="text-xs font-bold text-surface-400 uppercase tracking-widest">Carga Lectiva</h4>
                  <div className="p-3 bg-brand-50 rounded-xl flex items-start gap-3 border border-brand-100">
                    <Clock className="text-brand-600 mt-1" size={18} />
                    <div className="flex flex-col">
                      <span className="text-[10px] text-brand-600 uppercase font-bold">Horas Semanales Máx.</span>
                      <span className="text-lg font-bold text-brand-900">{selectedTeacher.maxWorkingHours}h</span>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Availability Modal */}
      <AnimatePresence>
        {showAvailability && selectedTeacher && (
          <div className="fixed inset-0 z-[110] flex items-center justify-center p-4">
             <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setShowAvailability(false)}
              className="absolute inset-0 bg-surface-900/60 backdrop-blur-md"
            />
            <motion.div 
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-5xl relative z-10 overflow-hidden h-[85vh] flex flex-col"
            >
              <div className="p-8 border-b border-surface-100 flex items-center justify-between">
                <div>
                  <h3 className="text-2xl font-black text-surface-900">Disponibilidad: {selectedTeacher.firstName}</h3>
                  <p className="text-surface-500 font-medium">Define las preferencias horarias del docente</p>
                </div>
                <button onClick={() => setShowAvailability(false)} className="p-3 bg-surface-100 hover:bg-surface-200 rounded-2xl transition-colors">
                  <X size={20} />
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-8">
                <TeacherAvailabilityGrid 
                  teacherId={selectedTeacher.id} 
                  timeSlots={timeSlots} 
                  onSave={() => {
                    setShowAvailability(false)
                    loadTeachers()
                  }}
                />
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

      {/* Form Modal */}
      <AnimatePresence>
        {showForm && (
          <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={resetForm}
              className="absolute inset-0 bg-surface-900/40 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl relative z-10 overflow-hidden"
            >
              <div className="p-6 border-b border-surface-100 flex items-center justify-between bg-surface-50">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-brand-100 text-brand-600 flex items-center justify-center">
                    <UserSquare2 size={24} />
                  </div>
                  <div>
                    <h3 className="text-xl font-bold">{editing ? 'Editar Profesor' : 'Nuevo Profesor'}</h3>
                    <p className="text-sm text-surface-500">Completa la ficha técnica del docente</p>
                  </div>
                </div>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6 max-h-[75vh] overflow-y-auto">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                       <User size={14} className="text-surface-400" /> Nombre
                    </label>
                    <input 
                      value={form.firstName} 
                      onChange={e => setForm({ ...form, firstName: e.target.value })} 
                      placeholder="Nombre" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-sm font-bold text-surface-700">Apellidos</label>
                    <input 
                      value={form.lastName} 
                      onChange={e => setForm({ ...form, lastName: e.target.value })} 
                      placeholder="Apellidos" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                      <Mail size={14} className="text-surface-400" /> Email
                    </label>
                    <input 
                      type="email"
                      value={form.email} 
                      onChange={e => setForm({ ...form, email: e.target.value })} 
                      placeholder="email@colegio.es" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                      <Phone size={14} className="text-surface-400" /> Teléfono
                    </label>
                    <input 
                      value={form.phone} 
                      onChange={e => setForm({ ...form, phone: e.target.value })} 
                      placeholder="600 000 000" 
                      className="input-field w-full" 
                      required 
                    />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                     <BookOpen size={14} className="text-surface-400" /> Especialidad
                  </label>
                  <input 
                    value={form.specialty} 
                    onChange={e => setForm({ ...form, specialty: e.target.value })} 
                    placeholder="Ej. Matemáticas, Primaria, Inglés..." 
                    className="input-field w-full" 
                  />
                </div>

                {/* Advanced Preferences */}
                <div className="space-y-4 pt-4 border-t border-surface-100">
                  <h4 className="text-sm font-black text-brand-600 uppercase tracking-widest flex items-center gap-2">
                    <Settings2 size={16} /> Opciones Avanzadas de Horario
                  </h4>
                  
                  <div className="grid grid-cols-2 gap-6">
                    <div className="space-y-1.5">
                      <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                        <Clock size={14} className="text-surface-400" /> Carga Semanal (h)
                      </label>
                      <input 
                        type="number"
                        value={form.maxWorkingHours} 
                        onChange={e => setForm({ ...form, maxWorkingHours: parseInt(e.target.value) || 0 })} 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                        <LayoutGrid size={14} className="text-surface-400" /> Huecos Máx/Día
                      </label>
                      <input 
                        type="number"
                        value={form.maxGapsPerDay} 
                        onChange={e => setForm({ ...form, maxGapsPerDay: parseInt(e.target.value) || 0 })} 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-bold text-surface-700 flex items-center gap-2">
                        <Clock size={14} className="text-surface-400" /> Mín. Horas/Día
                      </label>
                      <input 
                        type="number"
                        value={form.minDailyHours} 
                        onChange={e => setForm({ ...form, minDailyHours: parseInt(e.target.value) || 0 })} 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-bold text-surface-700">Día Libre Preferido</label>
                      <select 
                        value={form.preferredFreeDay ?? ''}
                        onChange={e => setForm({ ...form, preferredFreeDay: e.target.value ? parseInt(e.target.value) : undefined })}
                        className="input-field w-full"
                      >
                        <option value="">Cualquiera</option>
                        <option value="0">Lunes</option>
                        <option value="1">Martes</option>
                        <option value="2">Miércoles</option>
                        <option value="3">Jueves</option>
                        <option value="4">Viernes</option>
                      </select>
                    </div>
                  </div>

                  <div className="flex items-center justify-between p-4 bg-surface-50 rounded-2xl border border-surface-100">
                    <div className="flex items-center gap-3">
                      <div className="p-2 bg-white rounded-lg border border-surface-200">
                        <Info size={16} className="text-brand-600" />
                      </div>
                      <div className="flex flex-col">
                        <span className="text-sm font-bold text-surface-900">Preferir Horario Compacto</span>
                        <span className="text-[10px] text-surface-500">Minimiza huecos entre clases para evitar tiempos muertos</span>
                      </div>
                    </div>
                    <button 
                      type="button"
                      onClick={() => setForm({ ...form, preferCompactSchedule: !form.preferCompactSchedule })}
                      className={`w-12 h-6 rounded-full transition-all relative ${form.preferCompactSchedule ? 'bg-brand-600' : 'bg-surface-300'}`}
                    >
                      <div className={`absolute top-1 w-4 h-4 bg-white rounded-full transition-all ${form.preferCompactSchedule ? 'left-7' : 'left-1'}`} />
                    </button>
                  </div>
                </div>

                <div className="flex gap-3 pt-6">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1 py-3">
                    {editing ? 'Guardar Cambios' : 'Registrar Profesor'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}