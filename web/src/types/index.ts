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

export interface Room {
  id: string
  name: string
  type: number
  capacity: number
  building?: string
  floor?: number
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
  maxWorkingHours: number
  maxGapsPerDay: number
  minDailyHours: number
  preferCompactSchedule: boolean
  preferredFreeDay?: number
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

export interface Subject {
  id: string
  name: string
  color?: string
}

export interface ConflictInfo {
  type: string
  description: string
  severity: 'Error' | 'Warning'
  teacherId?: string
  roomId?: string
  timeSlotId?: string
  classUnitId?: string
}

export interface ScheduleScore {
  totalScore: number
  teacherSatisfaction: number
  compactnessScore: number
  balanceScore: number
  coverageScore: number
  details: string[]
}

export interface Curriculum {
  gradeLevel: string
  subjects: {
    name: string
    weeklyHours: number
  }[]
}

export type AcademicSessionType = 0 | 1;
export const AcademicSession = {
  Standard: 0,
  Intensive: 1
} as const;

export type DayOfWeek = 0 | 1 | 2 | 3 | 4;
export const Days = {
  Monday: 0,
  Tuesday: 1,
  Wednesday: 2,
  Thursday: 3,
  Friday: 4
} as const;

export interface TimeSlot {
  id: string
  sessionType: AcademicSessionType
  dayOfWeek: DayOfWeek
  startTime: string
  endTime: string
  isBreak: boolean
  label: string
}

export interface Schedule {
  id: string
  classroomId: string
  teacherId: string
  subjectId: string
  timeSlotId: string
  isLocked: boolean
  teacher?: Teacher
  subject?: Subject
  timeSlot?: TimeSlot
  classroom?: Classroom
  room?: Room
}

export interface ClassUnit {
  id: string
  classroomId: string
  classroomName: string
  gradeLevel: number
  line: string
  subjectId: string
  subjectName: string
  subjectColor: string
  teacherId: string | null
  teacherName: string | null
  weeklySessions: number
  sessionDuration: number
  allowConsecutiveDays: boolean
  preferNonConsecutive: boolean
  allowDoubleSession: boolean
  maxSessionsPerDay: number
  preferredRoomId: string | null
  preferredRoomName: string | null
  simultaneousGroupId: string | null
  isActive: boolean
}

export interface BreakDefinition {
  id: string
  timetableFrameworkId: string
  startTime: string
  endTime: string
  label: string
}

export interface TimetableFramework {
  id: string
  name: string
  sessionType: AcademicSessionType
  hasAfternoon: boolean
  morningStart: string
  morningEnd: string
  afternoonStart?: string
  afternoonEnd?: string
  sessionDurationMinutes: number
  breaks: BreakDefinition[]
}