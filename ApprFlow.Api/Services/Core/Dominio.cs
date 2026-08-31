namespace ApprFlow.Api.Services.Core
{
    public static class Dominio
    {
        public const int MIN_VAL = 1;
        public enum TipoRol {
            ADM = 1, USR = 2
        }
        public enum TipoEdo {
            PENDING = 1, APPROVED = 2, REJECTED = 3, BLOCKED = 4
        }
        public enum TipoEx {
            GRAL, BD, LOGIC, BR, NULL
        }
        public enum TipoOp {
            INS, REP, UPD, DEL, BR
        }
        public record Error(string Codigo, string Mensaje, string Detalle);

        public static string Mensaje(TipoEx tipo)
        {
            return tipo switch {
                TipoEx.GRAL => "Error general",
                TipoEx.BD => "Error de BD",
                TipoEx.LOGIC => "Error lógico",
                TipoEx.BR => "Error de regla de negocio",
                TipoEx.NULL => "Error de valor nulo",
                _ => "Error desconocido"
            };
        }
        public static string Codigo(TipoOp tipo)
        {
            return tipo switch {
                TipoOp.INS => "ERR_INSERT",
                TipoOp.REP => "ERR_REPLACE",
                TipoOp.UPD => "ERR_UPDATE",
                TipoOp.DEL => "ERR_DELETE",
                TipoOp.BR  => "ERR_BZ_RULE",
                _ => "ERR_UNKNOWN"
            };
        }
    }
}
