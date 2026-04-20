import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  MoreHorizontal, 
  Edit2, 
  Trash2, 
  Phone, 
  Mail, 
  MapPin,
  X
} from 'lucide-react'
import { schoolsApi } from '../services/api'
import type { School } from '../types'

export default function Schools() {
  const [schools, setSchools] = useState<School[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<School | null>(null)
  const [form, setForm] = useState({ 
    name: '', 
    cif: '', 
    address: '', 
    city: '', 
    postalCode: '', 
    province: '', 
    contactPhone: '', 
    contactEmail: '' 
  })
  const [selectedSchool, setSelectedSchool] = useState<School | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadSchools()
  }, [])

  const loadSchools = async () => {
    try {
      const res = await schoolsApi.getAll()
      setSchools(res.data)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editing) {
        await schoolsApi.update(editing.id, form)
      } else {
        await schoolsApi.create(form)
      }
      resetForm()
      loadSchools()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ 
      name: '', 
      cif: '', 
      address: '', 
      city: '', 
      postalCode: '', 
      province: '', 
      contactPhone: '', 
      contactEmail: '' 
    })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Estás seguro de que deseas eliminar este colegio?')) {
      try {
        await schoolsApi.delete(id)
        loadSchools()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (school: School) => {
    setEditing(school)
    setForm({ 
      name: school.name, 
      cif: school.cif, 
      address: school.address, 
      city: school.city, 
      postalCode: school.postalCode, 
      province: school.province, 
      contactPhone: school.contactPhone, 
      contactEmail: school.contactEmail 
    })
    setShowForm(true)
  }

  const filteredSchools = schools.filter(s => 
    s.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    s.contactEmail.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      {/* Action Bar */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={18} />
          <input 
            type="text" 
            placeholder="Buscar colegios..." 
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
          Nuevo Colegio
        </button>
      </div>

      {/* Main Content */}
      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando instituciones...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Institución</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Localización</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Contacto</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filteredSchools.map((school) => (
                    <motion.tr 
                      key={school.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      exit={{ opacity: 0 }}
                      className="hover:bg-surface-50/50 transition-colors group"
                    >
                      <td className="px-6 py-4">
                        <div 
                          className="flex items-center gap-3 cursor-pointer group/school"
                          onClick={() => setSelectedSchool(school)}
                        >
                          <div className="w-10 h-10 rounded-xl bg-brand-50 flex items-center justify-center text-brand-600 font-bold group-hover/school:bg-brand-600 group-hover/school:text-white transition-all">
                            {school.name.charAt(0)}
                          </div>
                          <div className="flex flex-col">
                            <span className="font-semibold text-surface-900 line-clamp-1">{school.name}</span>
                            <span className="text-[10px] bg-surface-100 text-surface-500 px-1.5 py-0.5 rounded-full w-fit uppercase font-bold tracking-wider">CIF: {school.cif || 'N/A'}</span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-sm text-surface-600">
                        <div className="space-y-0.5">
                          <div className="flex items-center gap-1.5">
                            <MapPin size={14} className="text-surface-400" />
                            {school.address || "No definida"}
                          </div>
                          <div className="pl-5 text-xs text-surface-400">
                            {school.postalCode} {school.city} {school.city && school.province ? '-' : ''} {school.province}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="space-y-1">
                          <div className="flex items-center gap-1.5 text-xs text-surface-600">
                            <Mail size={12} className="text-surface-400" />
                            {school.contactEmail}
                          </div>
                          <div className="flex items-center gap-1.5 text-xs text-surface-600">
                            <Phone size={12} className="text-surface-400" />
                            {school.contactPhone}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button 
                            onClick={() => handleEdit(school)}
                            className="p-2 hover:bg-brand-50 text-brand-600 rounded-lg transition-colors"
                            title="Editar"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={() => handleDelete(school.id)}
                            className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                            title="Eliminar"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filteredSchools.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-6 py-12 text-center text-surface-400">
                      No se encontraron instituciones.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modern Modal / Slide-over for Form */}
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
                <h3 className="text-xl font-bold">{editing ? 'Editar Colegio' : 'Nuevo Colegio'}</h3>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="md:col-span-2 space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Nombre de la Institución</label>
                      <input 
                        value={form.name} 
                        onChange={e => setForm({ ...form, name: e.target.value })} 
                        placeholder="Ej. Colegio San José" 
                        className="input-field w-full" 
                        required 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">CIF / NIF</label>
                      <input 
                        value={form.cif} 
                        onChange={e => setForm({ ...form, cif: e.target.value })} 
                        placeholder="Q2868006E" 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Dirección</label>
                      <input 
                        value={form.address} 
                        onChange={e => setForm({ ...form, address: e.target.value })} 
                        placeholder="Calle, Número..." 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Ciudad</label>
                      <input 
                        value={form.city} 
                        onChange={e => setForm({ ...form, city: e.target.value })} 
                        placeholder="Ciudad" 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Provincia</label>
                      <input 
                        value={form.province} 
                        onChange={e => setForm({ ...form, province: e.target.value })} 
                        placeholder="Provincia" 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Código Postal</label>
                      <input 
                        value={form.postalCode} 
                        onChange={e => setForm({ ...form, postalCode: e.target.value })} 
                        placeholder="28000" 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Teléfono</label>
                      <input 
                        value={form.contactPhone} 
                        onChange={e => setForm({ ...form, contactPhone: e.target.value })} 
                        placeholder="+34 000 000 000" 
                        className="input-field w-full" 
                      />
                    </div>
                    <div className="md:col-span-2 space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Email de Contacto</label>
                      <input 
                        type="email"
                        value={form.contactEmail} 
                        onChange={e => setForm({ ...form, contactEmail: e.target.value })} 
                        placeholder="admin@colegio.com" 
                        className="input-field w-full" 
                      />
                    </div>
                  </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">
                    {editing ? 'Guardar Cambios' : 'Crear Institución'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

      {/* Detailed School Profile Modal */}
      <AnimatePresence>
        {selectedSchool && (
          <div className="fixed inset-0 z-[110] flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setSelectedSchool(null)}
              className="absolute inset-0 bg-surface-900/60 backdrop-blur-md"
            />
            <motion.div 
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-white rounded-3xl shadow-2xl w-full max-w-4xl relative z-10 overflow-hidden flex flex-col md:flex-row h-[90vh] md:h-auto max-h-[90vh]"
            >
              {/* Left Side: Info */}
              <div className="flex-1 p-8 overflow-y-auto space-y-8">
                <div className="flex justify-between items-start">
                  <div className="flex items-center gap-4">
                    <div className="w-16 h-16 rounded-2xl bg-brand-600 text-white flex items-center justify-center text-2xl font-bold shadow-lg shadow-brand-200">
                      {selectedSchool.name.charAt(0)}
                    </div>
                    <div>
                      <h2 className="text-2xl font-black text-surface-900">{selectedSchool.name}</h2>
                      <p className="text-surface-500 font-medium">CIF: {selectedSchool.cif}</p>
                    </div>
                  </div>
                  <button onClick={() => setSelectedSchool(null)} className="p-2 hover:bg-surface-100 rounded-full transition-colors">
                    <X size={24} />
                  </button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <h3 className="text-xs font-bold text-surface-400 uppercase tracking-widest">Ubicación</h3>
                    <div className="space-y-3">
                      <div className="flex items-start gap-3">
                        <MapPin className="text-brand-500 shrink-0" size={20} />
                        <div>
                          <p className="text-surface-900 font-semibold">{selectedSchool.address}</p>
                          <p className="text-surface-500 text-sm">{selectedSchool.postalCode} {selectedSchool.city}</p>
                          <p className="text-surface-500 text-sm">{selectedSchool.province}</p>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <h3 className="text-xs font-bold text-surface-400 uppercase tracking-widest">Contacto</h3>
                    <div className="space-y-3">
                      <a href={`tel:${selectedSchool.contactPhone}`} className="flex items-center gap-3 hover:text-brand-600 transition-colors group">
                        <div className="w-10 h-10 rounded-full bg-surface-50 flex items-center justify-center group-hover:bg-brand-50 transition-colors">
                          <Phone size={18} className="text-surface-400 group-hover:text-brand-500" />
                        </div>
                        <span className="text-surface-700 font-medium">{selectedSchool.contactPhone}</span>
                      </a>
                      <a href={`mailto:${selectedSchool.contactEmail}`} className="flex items-center gap-3 hover:text-brand-600 transition-colors group">
                        <div className="w-10 h-10 rounded-full bg-surface-50 flex items-center justify-center group-hover:bg-brand-50 transition-colors">
                          <Mail size={18} className="text-surface-400 group-hover:text-brand-500" />
                        </div>
                        <span className="text-surface-700 font-medium">{selectedSchool.contactEmail}</span>
                      </a>
                    </div>
                  </div>
                </div>

                <div className="pt-4 flex gap-4">
                  <button 
                    onClick={() => { handleEdit(selectedSchool); setSelectedSchool(null); }}
                    className="btn-secondary flex-1 flex items-center justify-center gap-2"
                  >
                    <Edit2 size={18} />
                    Editar Datos
                  </button>
                  <a 
                    href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${selectedSchool.name} ${selectedSchool.address} ${selectedSchool.city}`)}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn-primary flex-1 flex items-center justify-center gap-2"
                  >
                    <MapPin size={18} />
                    Ver en Maps
                  </a>
                </div>
              </div>

              {/* Right Side: Google Maps Embed */}
              <div className="w-full md:w-1/2 bg-surface-100 min-h-[300px] md:min-h-full relative overflow-hidden">
                <iframe 
                  width="100%" 
                  height="100%" 
                  style={{ border: 0 }} 
                  loading="lazy" 
                  allowFullScreen 
                  referrerPolicy="no-referrer-when-downgrade"
                  className="absolute inset-0"
                  src={`https://maps.google.com/maps?q=${encodeURIComponent(`${selectedSchool.address}, ${selectedSchool.city}, ${selectedSchool.province}`)}&t=&z=15&ie=UTF8&iwloc=&output=embed`}
                ></iframe>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}