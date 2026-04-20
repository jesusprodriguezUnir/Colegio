export interface School {
  id: string
  name: string
  cif: string
  address: string
  city: string
  postalCode: string
  province: string
  contactPhone: string
  contactEmail: string
}

export interface Teacher {
  id: string
  firstName: string
  lastName: string
  specialty: string
  email: string
  phone: string
  iban: string
  dateOfBirth: string
  hireDate: string
  tutorOf?: {
    gradeLevel: number
    line: string
  }
}

export interface Student {
  id: string
  firstName: string
  lastName: string
  dateOfBirth: string
  classroomId?: string
}

export interface Classroom {
  id: string
  gradeLevel: number
  line: string
  schoolId: string
  tutorId?: string
  tutor?: Teacher
  students?: Student[]
}

export interface Invoice {
  id: string
  parentId: string
  studentId: string
  issueDate: string
  dueDate: string
  totalAmount: number
  status: 'Pending' | 'Paid'
  concept: 'Monthly' | 'Lunch' | 'Extracurricular'
}