export interface TeachingActivity {
  postNumber: number;
  postType: string;
  courseHours: number;
  seminarHours: number;
  labHours: number;
  projectHours: number;
  type: string;
  group: string;
  day: string;
  parity: 'Impar' | 'Par' | null;
  subject: string;
  startWeek: string | null;
  totalHours: number | null;
}