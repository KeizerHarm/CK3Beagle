import { Uri } from "vscode";

export type ServerResponse = BasicResponse | ErrorResponse | AnalysisResponse | InitialAnalysisResponse | MedianAnalysisResponse | FinalAnalysisResponse;

interface BaseMessage {
  type: string;
}

interface BasicResponse extends BaseMessage {
  type: 'basic';
  payload: {
    message: string;
  }
}

interface ErrorResponse extends BaseMessage {
  type: 'error';
  payload: {
    message: string;
  };
}

export interface AnalysisResponse extends BaseMessage {
  type: 'analysis';
  payload: {
    summary: string;
    smells: Smell[];
  }
}

interface InitialAnalysisResponse extends BaseMessage {
  type: 'analysis_initial';
  payload: {
    message: string;
  }
}
export interface MedianAnalysisResponse extends BaseMessage {
  type: 'analysis_median';
  payload: {
    smells: Smell[];
    message: string;
  }
}
interface FinalAnalysisResponse extends BaseMessage {
  type: 'analysis_final';
  payload: {
    message: string;
  }
}

export interface Smell {
  severity: number;
  file: string;
  startLine: number;
  endLine: number;
  startIndex: number;
  endIndex: number;
  message: string;
  code: {
    value: string;
    target: string;
  };
  relatedLogEntries: RelatedSmell[];
}

export interface RelatedSmell {
  file: string;
  startLine: number;
  endLine: number;
  startIndex: number;
  endIndex: number;
  message: string;
}