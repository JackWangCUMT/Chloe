﻿//#if DEBUG
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Chloe.Query;
//using Chloe.DbExpressions;
//using System.Reflection;
//using Chloe.Extensions;
//using System.Collections;
//using Chloe.Utility;
//using System.Collections.ObjectModel;
//using Chloe.Core.Visitors;
//using Chloe.Core;

//namespace Chloe.SqlServer
//{
//    class SqlExpressionVisitor : DbExpressionVisitor<ISqlState>
//    {
//        public const string ParameterPrefix = "@P_";

//        DbColumnExpressionVisitor _columnExpressionVisitor = null;

//        protected List<DbParam> _parameters = new List<DbParam>();
//        protected Dictionary<object, SqlState> _innerParameterStorage = new Dictionary<object, SqlState>();

//        static readonly Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>> MethodHandlers = InitMethodHandlers();
//        static readonly Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>> FuncHandlers = InitFuncHandlers();
//        static readonly Dictionary<MethodInfo, Func<DbBinaryExpression, SqlExpressionVisitor, ISqlState>> BinaryWithMethodHandlers = InitBinaryWithMethodHandlers();
//        static readonly Dictionary<Type, string> CSharpType_DbType_Mappings = null;

//        public static readonly ReadOnlyCollection<DbExpressionType> SafeDbExpressionTypes = null;

//        static readonly List<Tuple<string, SqlState>> CacheParameterSqlStates = null;

//        static SqlExpressionVisitor()
//        {
//            List<DbExpressionType> list = new List<DbExpressionType>();
//            list.Add(DbExpressionType.MemberAccess);
//            list.Add(DbExpressionType.ColumnAccess);
//            list.Add(DbExpressionType.Constant);
//            list.Add(DbExpressionType.Parameter);
//            list.Add(DbExpressionType.Convert);
//            SafeDbExpressionTypes = list.AsReadOnly();

//            Dictionary<Type, string> cSharpType_DbType_Mappings = new Dictionary<Type, string>(8);
//            cSharpType_DbType_Mappings.Add(typeof(string), "NVARCHAR(MAX)");
//            cSharpType_DbType_Mappings.Add(typeof(Int16), "SMALLINT");
//            cSharpType_DbType_Mappings.Add(typeof(int), "INT");
//            cSharpType_DbType_Mappings.Add(typeof(long), "BIGINT");
//            cSharpType_DbType_Mappings.Add(typeof(decimal), "DECIMAL");
//            cSharpType_DbType_Mappings.Add(typeof(double), "FLOAT");
//            cSharpType_DbType_Mappings.Add(typeof(float), "REAL");
//            cSharpType_DbType_Mappings.Add(typeof(bool), "BIT");

//            cSharpType_DbType_Mappings.Add(typeof(Int16?), "SMALLINT");
//            cSharpType_DbType_Mappings.Add(typeof(int?), "INT");
//            cSharpType_DbType_Mappings.Add(typeof(long?), "BIGINT");
//            cSharpType_DbType_Mappings.Add(typeof(decimal?), "DECIMAL");
//            cSharpType_DbType_Mappings.Add(typeof(double?), "FLOAT");
//            cSharpType_DbType_Mappings.Add(typeof(float?), "REAL");
//            cSharpType_DbType_Mappings.Add(typeof(bool?), "BIT");

//            cSharpType_DbType_Mappings.Add(typeof(DateTime), "DATETIME");
//            cSharpType_DbType_Mappings.Add(typeof(DateTime?), "DATETIME");

//            cSharpType_DbType_Mappings.Add(typeof(Guid), "UNIQUEIDENTIFIER");
//            cSharpType_DbType_Mappings.Add(typeof(Guid?), "UNIQUEIDENTIFIER");

//            CSharpType_DbType_Mappings = cSharpType_DbType_Mappings;

//            int cacheParameterSqlStateCount = 2 * 12;
//            List<Tuple<string, SqlState>> cacheParameterSqlStates = new List<Tuple<string, SqlState>>(cacheParameterSqlStateCount);

//            for (int i = 0; i < cacheParameterSqlStateCount; i++)
//            {
//                string paramName = ParameterPrefix + i.ToString();
//                Tuple<string, SqlState> tuple = new Tuple<string, SqlState>(paramName, SqlState.Create(paramName));
//                cacheParameterSqlStates.Add(tuple);
//            }

//            CacheParameterSqlStates = cacheParameterSqlStates;
//        }

//        public List<DbParam> Parameters { get { return this._parameters; } }

//        DbColumnExpressionVisitor ColumnExpressionVisitor
//        {
//            get
//            {
//                if (this._columnExpressionVisitor == null)
//                    this._columnExpressionVisitor = new DbColumnExpressionVisitor(this);

//                return this._columnExpressionVisitor;
//            }
//        }

//        public static SqlExpressionVisitor CreateInstance()
//        {
//            return new SqlExpressionVisitor();
//        }

//        public override ISqlState Visit(DbEqualExpression exp)
//        {
//            DbExpression left = exp.Left;
//            DbExpression right = exp.Right;

//            left = DbExpressionExtensions.ParseDbExpression(left);
//            right = DbExpressionExtensions.ParseDbExpression(right);

//            //明确 left right 其中一边一定为 null
//            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
//            {
//                return SqlState.Create(left.Accept(this), " IS NULL");
//            }

//            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
//            {
//                return SqlState.Create(right.Accept(this), " IS NULL");
//            }

//            ISqlState leftState = left.Accept(this);
//            ISqlState rightState = right.Accept(this);

//            return SqlState.Create(leftState, " = ", rightState);
//        }
//        public override ISqlState Visit(DbNotEqualExpression exp)
//        {
//            DbExpression left = exp.Left;
//            DbExpression right = exp.Right;

//            left = DbExpressionExtensions.ParseDbExpression(left);
//            right = DbExpressionExtensions.ParseDbExpression(right);

//            //明确 left right 其中一边一定为 null
//            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
//            {
//                return SqlState.Create(left.Accept(this), " IS NOT NULL");
//            }

//            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
//            {
//                return SqlState.Create(right.Accept(this), " IS NOT NULL");
//            }

//            ISqlState leftState = left.Accept(this);
//            ISqlState rightState = right.Accept(this);

//            return SqlState.Create(leftState, " <> ", rightState);
//        }

//        public override ISqlState Visit(DbNotExpression exp)
//        {
//            return SqlState.Create("NOT ", "(", exp.Operand.Accept(this), ")");
//        }

//        public override ISqlState Visit(DbAndAlsoExpression exp)
//        {
//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " AND ");
//        }
//        public override ISqlState Visit(DbOrElseExpression exp)
//        {
//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " OR ");
//        }

//        public override ISqlState Visit(DbConvertExpression exp)
//        {
//            DbExpression stripedExp = DbExpressionHelper.StripInvalidConvert(exp);

//            if (stripedExp.NodeType != DbExpressionType.Convert)
//            {
//                return EnsureDbExpressionReturnCSharpBoolean(stripedExp).Accept(this);
//            }

//            exp = (DbConvertExpression)stripedExp;

//            string dbTypeString;
//            if (!CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
//            {
//                throw new NotSupportedException(string.Format("不支持将类型 {0} 转换为 {1}", exp.Operand.Type.Name, exp.Type.Name));
//            }

//            SqlState state = BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this), dbTypeString);

//            return state;
//        }
//        // +
//        public override ISqlState Visit(DbAddExpression exp)
//        {
//            MethodInfo method = exp.Method;
//            if (method != null)
//            {
//                Func<DbBinaryExpression, SqlExpressionVisitor, ISqlState> handler;
//                if (BinaryWithMethodHandlers.TryGetValue(method, out handler))
//                {
//                    return handler(exp, this);
//                }

//                throw new NotSupportedException(string.Format("{0}.{1}", method.DeclaringType.FullName, exp.Method.Name));
//            }

//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " + ");
//        }
//        // -
//        public override ISqlState Visit(DbSubtractExpression exp)
//        {
//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " - ");
//        }
//        // *
//        public override ISqlState Visit(DbMultiplyExpression exp)
//        {
//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " * ");
//        }
//        // /
//        public override ISqlState Visit(DbDivideExpression exp)
//        {
//            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
//            return this.ConcatOperands(operands, " / ");
//        }
//        // <
//        public override ISqlState Visit(DbLessThanExpression exp)
//        {
//            return SqlState.Create(exp.Left.Accept(this), " < ", exp.Right.Accept(this));
//        }
//        // <=
//        public override ISqlState Visit(DbLessThanOrEqualExpression exp)
//        {
//            return SqlState.Create(exp.Left.Accept(this), " <= ", exp.Right.Accept(this));
//        }
//        // >
//        public override ISqlState Visit(DbGreaterThanExpression exp)
//        {
//            return SqlState.Create(exp.Left.Accept(this), " > ", exp.Right.Accept(this));
//        }
//        // >=
//        public override ISqlState Visit(DbGreaterThanOrEqualExpression exp)
//        {
//            return SqlState.Create(exp.Left.Accept(this), " >= ", exp.Right.Accept(this));
//        }

//        public override ISqlState Visit(DbConstantExpression exp)
//        {
//            if (exp.Value == null || exp.Value == DBNull.Value)
//            {
//                return SqlState.Create("NULL");
//            }

//            var objType = exp.Value.GetType();
//            if (objType == UtilConstants.TypeOfBoolean)
//            {
//                return SqlState.Create(((bool)exp.Value) ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)");
//            }
//            else if (objType == UtilConstants.TypeOfString)
//            {
//                return SqlState.Create("N'", exp.Value, "'");
//            }
//            else if (objType.IsEnum)
//            {
//                return SqlState.Create(((int)exp.Value).ToString());
//            }

//            return SqlState.Create(exp.Value);
//        }

//        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
//        public override ISqlState Visit(DbCaseWhenExpression exp)
//        {
//            SqlState state = new SqlState(4 + exp.WhenThenExps.Count * 4);
//            state.Append("CASE");
//            foreach (var item in exp.WhenThenExps)
//            {
//                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
//                state.Append(" WHEN ", item.When.Accept(this), " THEN ", EnsureDbExpressionReturnCSharpBoolean(item.Then).Accept(this));
//            }
//            state.Append(" ELSE ", EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this), " END");

//            return state;
//        }

//        public override ISqlState Visit(DbTableExpression exp)
//        {
//            return QuoteName(exp.Table.Name);
//        }

//        public override ISqlState Visit(DbColumnAccessExpression exp)
//        {
//            return SqlState.Create(QuoteName(exp.Table.Name), ".", QuoteName(exp.Column.Name));
//        }

//        public override ISqlState Visit(DbMemberExpression exp)
//        {
//            ISqlState sqlState;
//            if (IsDbFunction_DATEDIFF(exp, this, out sqlState))
//            {
//                return sqlState;
//            }

//            MemberInfo member = exp.Member;

//            if (member.DeclaringType == UtilConstants.TypeOfDateTime)
//            {
//                if (member == UtilConstants.PropertyInfo_DateTime_Now)
//                {
//                    return SqlState.Create("GETDATE()");
//                }

//                if (member == UtilConstants.PropertyInfo_DateTime_UtcNow)
//                {
//                    return SqlState.Create("GETUTCDATE()");
//                }

//                if (member == UtilConstants.PropertyInfo_DateTime_Today)
//                {
//                    return BuildCastState("GETDATE()", "DATE");
//                }

//                if (member == UtilConstants.PropertyInfo_DateTime_Date)
//                {
//                    return BuildCastState(exp.Expression.Accept(this), "DATE");
//                }

//                if (IsDbFunction_DATEPART(exp, this, out sqlState))
//                {
//                    return sqlState;
//                }
//            }


//            DbParameterExpression newExp;
//            if (DbExpressionExtensions.TryParseToParameterExpression(exp, out newExp))
//            {
//                return newExp.Accept(this);
//            }

//            if (member.Name == "Length" && member.DeclaringType == typeof(string))
//            {
//                return SqlState.Create("LEN(", exp.Expression.Accept(this), ")");
//            }
//            else if (member.Name == "Value" && Utils.IsNullable(exp.Expression.Type))
//            {
//                return exp.Expression.Accept(this);
//            }

//            throw new NotSupportedException(member.Name);
//        }
//        public override ISqlState Visit(DbParameterExpression exp)
//        {
//            object val = exp.Value;
//            if (val == null)
//                val = DBNull.Value;

//            SqlState state;
//            if (val == DBNull.Value)
//            {
//                var p = this._parameters.Where(a => a.Value == val && a.Type == exp.Type).FirstOrDefault();
//                if (p != null)
//                {
//                    return SqlState.Create(p.Name);
//                }

//                string paramName;
//                state = GetParameterNameSqlState(this._parameters.Count, out paramName);
//                this._parameters.Add(DbParam.Create(paramName, val, exp.Type));
//                return state;
//            }

//            if (!this._innerParameterStorage.TryGetValue(val, out state))
//            {
//                string paramName;
//                state = GetParameterNameSqlState(this._parameters.Count, out paramName);

//                this._innerParameterStorage.Add(val, state);
//                this._parameters.Add(DbParam.Create(paramName, val, exp.Type));
//            }

//            return state;
//        }

//        public override ISqlState Visit(DbSubQueryExpression exp)
//        {
//            ISqlState state = exp.SqlQuery.Accept(this);
//            return BracketState(state);
//        }
//        public override ISqlState Visit(DbSqlQueryExpression exp)
//        {
//            if (exp.TakeCount != null && exp.SkipCount != null)
//            {
//                //构建分页查询
//                return this.BuildLimitSqlState(exp);
//            }
//            else if (exp.TakeCount != null)
//            {
//                //构建 top 查询
//                return this.BuildTakeSqlState(exp);
//            }
//            else if (exp.SkipCount != null)
//            {
//                //构建 skip 查询
//                return this.BuildSkipSqlState(exp);
//            }
//            else
//            {
//                //构建常规的查询
//                return this.BuildGeneralSqlState(exp);
//            }

//            throw new NotImplementedException();
//        }

//        public override ISqlState Visit(DbMethodCallExpression exp)
//        {
//            Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState> methodHandler;
//            if (!MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
//            {
//                throw UtilExceptions.NotSupportedMethod(exp.Method);
//            }
//            return methodHandler(exp, this);
//        }

//        public override ISqlState Visit(DbFromTableExpression exp)
//        {
//            return SqlState.Create(this.AppendTableSegment(exp.Table), this.VisitDbJoinTableExpressions(exp.JoinTables));
//        }

//        public override ISqlState Visit(DbJoinTableExpression exp)
//        {
//            DbJoinTableExpression joinTablePart = exp;
//            string joinString = null;

//            if (joinTablePart.JoinType == JoinType.InnerJoin)
//            {
//                joinString = " INNER JOIN ";
//            }
//            else if (joinTablePart.JoinType == JoinType.LeftJoin)
//            {
//                joinString = " LEFT JOIN ";
//            }
//            else if (joinTablePart.JoinType == JoinType.RightJoin)
//            {
//                joinString = " RIGHT JOIN ";
//            }
//            else if (joinTablePart.JoinType == JoinType.FullJoin)
//            {
//                joinString = " FULL JOIN ";
//            }
//            else
//                throw new NotSupportedException("JoinType: " + joinTablePart.JoinType);

//            return SqlState.Create(joinString, this.AppendTableSegment(joinTablePart.Table), " ON ", joinTablePart.Condition.Accept(this), this.VisitDbJoinTableExpressions(joinTablePart.JoinTables));
//        }

//        public override ISqlState Visit(DbFunctionExpression exp)
//        {
//            Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState> funcHandler;
//            if (!FuncHandlers.TryGetValue(exp.Method.Name, out funcHandler))
//            {
//                throw new NotSupportedException(exp.Method.Name);
//            }
//            return funcHandler(exp, this);
//        }

//        public override ISqlState Visit(DbInsertExpression exp)
//        {
//            SqlState state = new SqlState();
//            state.Append("INSERT INTO ", QuoteName(exp.Table.Name));
//            state.Append("(");

//            SqlState valuesState = new SqlState();
//            valuesState.Append(" VALUES(");

//            bool first = true;
//            foreach (var item in exp.InsertColumns)
//            {
//                if (first)
//                    first = false;
//                else
//                {
//                    state.Append(",");
//                    valuesState.Append(",");
//                }

//                state.Append(QuoteName(item.Key.Name));
//                valuesState.Append(item.Value.Accept(this.ColumnExpressionVisitor));
//            }
//            state.Append(")");
//            valuesState.Append(")");

//            state.Append(valuesState);
//            return state;
//        }
//        public override ISqlState Visit(DbUpdateExpression exp)
//        {
//            SqlState state = new SqlState();
//            state.Append("UPDATE ", QuoteName(exp.Table.Name), " SET ");

//            bool first = true;
//            foreach (var item in exp.UpdateColumns)
//            {
//                if (first)
//                    first = false;
//                else
//                    state.Append(",");

//                state.Append(QuoteName(item.Key.Name), "=", item.Value.Accept(this.ColumnExpressionVisitor));
//            }

//            state.Append(BuildWhereState(exp.Condition));

//            return state;
//        }
//        public override ISqlState Visit(DbDeleteExpression exp)
//        {
//            return SqlState.Create("DELETE ", QuoteName(exp.Table.Name), BuildWhereState(exp.Condition));
//        }

//        ISqlState AppendTableSegment(DbTableSegment seq)
//        {
//            ISqlState bodyState = seq.Body.Accept(this);
//            return SqlState.Create(bodyState, " AS ", QuoteName(seq.Alias));
//        }
//        ISqlState AppendColumnSegment(DbColumnSegment seq)
//        {
//            ISqlState bodyState = seq.Body.Accept(this.ColumnExpressionVisitor);
//            return SqlState.Create(bodyState, " AS ", QuoteName(seq.Alias));
//        }
//        ISqlState AppendOrderSegment(DbOrderSegment seq)
//        {
//            if (seq.OrderType == OrderType.Asc)
//                return SqlState.Create(seq.DbExpression.Accept(this), " ASC");
//            else if (seq.OrderType == OrderType.Desc)
//                return SqlState.Create(seq.DbExpression.Accept(this), " DESC");

//            throw new NotSupportedException("OrderType: " + seq.OrderType);
//        }

//        ISqlState VisitDbJoinTableExpressions(List<DbJoinTableExpression> tables)
//        {
//            SqlState state = new SqlState(tables.Count);
//            foreach (var table in tables)
//            {
//                state.Append(table.Accept(this));
//            }

//            return state;
//        }
//        ISqlState BuildGeneralSqlState(DbSqlQueryExpression exp)
//        {
//            SqlState columnsState = new SqlState();
//            List<DbColumnSegment> columns = exp.ColumnSegments;

//            for (int i = 0; i < columns.Count; i++)
//            {
//                DbColumnSegment column = columns[i];
//                if (i > 0)
//                    columnsState.Append(",");

//                columnsState.Append(this.AppendColumnSegment(column));
//            }

//            ISqlState fromTableState = exp.Table.Accept(this);

//            SqlState sqlState = new SqlState();
//            sqlState.Append("SELECT ", columnsState, " FROM ", fromTableState);

//            SqlState whereState = this.BuildWhereState(exp.Condition);
//            sqlState.Append(whereState);

//            if (exp.GroupSegments.Count > 0)
//            {
//                SqlState groupPartState = this.BuildGroupState(exp);
//                sqlState.Append(groupPartState);
//            }

//            SqlState orderState = this.BuildOrderState(exp.OrderSegments);
//            sqlState.Append(orderState);

//            return sqlState;
//        }
//        ISqlState BuildLimitSqlState(DbSqlQueryExpression exp)
//        {
//            SqlState columnsState = new SqlState();
//            List<DbColumnSegment> columns = exp.ColumnSegments;
//            List<ISqlState> columnStates = new List<ISqlState>(columns.Count);
//            for (int i = 0; i < columns.Count; i++)
//            {
//                DbColumnSegment column = columns[i];
//                if (i > 0)
//                    columnsState.Append(",");

//                ISqlState columnState = QuoteName(column.Alias);
//                columnsState.Append(column.Body.Accept(this.ColumnExpressionVisitor), " AS ", columnState);
//                columnStates.Add(columnState);
//            }

//            List<DbOrderSegment> orderSegs = exp.OrderSegments;
//            if (orderSegs.Count == 0)
//            {
//                DbOrderSegment orderSeg = new DbOrderSegment(UtilConstants.DbParameter_1, OrderType.Asc);
//                orderSegs = new List<DbOrderSegment>(1);
//                orderSegs.Add(orderSeg);
//            }

//            ISqlState fromTableState = exp.Table.Accept(this);

//            SqlState orderState = this.ConcatOrderSegments(orderSegs);

//            string row_numberName = CreateRowNumberName(columns);

//            SqlState row_numberNameState = QuoteName(row_numberName);
//            SqlState row_numberState = new SqlState();
//            row_numberState.Append("SELECT ", columnsState, ",ROW_NUMBER() OVER(ORDER BY ", orderState, ") AS ", row_numberNameState, " FROM ", fromTableState);

//            SqlState whereState = this.BuildWhereState(exp.Condition);
//            row_numberState.Append(whereState);

//            if (exp.GroupSegments.Count > 0)
//            {
//                SqlState groupPartState = this.BuildGroupState(exp);
//                row_numberState.Append(groupPartState);
//            }

//            string tableAlias = "T";
//            SqlState tableState_tableAlias = QuoteName(tableAlias);

//            SqlState selectedColumnState_TakeSql = new SqlState();
//            for (int i = 0; i < columnStates.Count; i++)
//            {
//                ISqlState columnState = columnStates[i];

//                if (i > 0)
//                    selectedColumnState_TakeSql.Append(",");

//                selectedColumnState_TakeSql.Append(tableState_tableAlias, ".", columnState, " AS ", columnState);
//            }

//            SqlState sqlState = SqlState.Create("SELECT TOP (", exp.TakeCount.ToString(), ") ", selectedColumnState_TakeSql, " FROM ", BracketState(row_numberState), " AS ", tableState_tableAlias, " WHERE ", tableState_tableAlias, ".", row_numberNameState, " > ", exp.SkipCount.ToString());

//            return sqlState;
//        }
//        ISqlState BuildTakeSqlState(DbSqlQueryExpression exp)
//        {
//            SqlState columnsState = new SqlState();
//            List<DbColumnSegment> columns = exp.ColumnSegments;

//            for (int i = 0; i < columns.Count; i++)
//            {
//                DbColumnSegment column = columns[i];
//                if (i > 0)
//                    columnsState.Append(",");

//                columnsState.Append(this.AppendColumnSegment(column));
//            }

//            List<DbOrderSegment> orderSegs = exp.OrderSegments;

//            ISqlState fromTableState = exp.Table.Accept(this);

//            SqlState sqlState = new SqlState();
//            sqlState.Append("SELECT TOP (", exp.TakeCount.Value.ToString(), ") ", columnsState, " FROM ", fromTableState);

//            SqlState whereState = this.BuildWhereState(exp.Condition);
//            sqlState.Append(whereState);

//            if (exp.GroupSegments.Count > 0)
//            {
//                SqlState groupPartState = this.BuildGroupState(exp);
//                sqlState.Append(groupPartState);
//            }

//            SqlState orderState = this.BuildOrderState(orderSegs);
//            sqlState.Append(orderState);

//            return sqlState;
//        }
//        ISqlState BuildSkipSqlState(DbSqlQueryExpression exp)
//        {
//            SqlState columnsState = new SqlState();
//            List<DbColumnSegment> columns = exp.ColumnSegments;
//            List<SqlState> columnStates = new List<SqlState>(columns.Count);
//            for (int i = 0; i < columns.Count; i++)
//            {
//                DbColumnSegment column = columns[i];
//                if (i > 0)
//                    columnsState.Append(",");

//                SqlState columnState = QuoteName(column.Alias);
//                columnsState.Append(column.Body.Accept(this.ColumnExpressionVisitor), " AS ", columnState);
//                columnStates.Add(columnState);
//            }

//            List<DbOrderSegment> orderSegs = exp.OrderSegments;
//            if (orderSegs.Count == 0)
//            {
//                DbOrderSegment orderSeg = new DbOrderSegment(UtilConstants.DbParameter_1, OrderType.Asc);
//                orderSegs = new List<DbOrderSegment>(1);
//                orderSegs.Add(orderSeg);
//            }

//            ISqlState fromTableState = exp.Table.Accept(this);
//            SqlState orderState = this.ConcatOrderSegments(orderSegs);

//            string row_numberName = CreateRowNumberName(columns);

//            SqlState row_numberNameState = QuoteName(row_numberName);
//            SqlState row_numberState = new SqlState();
//            row_numberState.Append("SELECT ", columnsState, ",ROW_NUMBER() OVER(ORDER BY ", orderState, ") AS ", row_numberNameState, " FROM ", fromTableState);

//            SqlState whereState = this.BuildWhereState(exp.Condition);
//            row_numberState.Append(whereState);

//            if (exp.GroupSegments.Count > 0)
//            {
//                SqlState groupPartState = this.BuildGroupState(exp);
//                row_numberState.Append(groupPartState);
//            }

//            string tableAlias = "T";
//            SqlState tableState_tableAlias = QuoteName(tableAlias);

//            SqlState selectedColumnState_TakeSql = new SqlState();
//            for (int i = 0; i < columnStates.Count; i++)
//            {
//                SqlState columnState = columnStates[i];

//                if (i > 0)
//                    selectedColumnState_TakeSql.Append(",");

//                selectedColumnState_TakeSql.Append(tableState_tableAlias, ".", columnState, " AS ", columnState);
//            }

//            SqlState sqlState = SqlState.Create("SELECT ", selectedColumnState_TakeSql, " FROM ", BracketState(row_numberState), " AS ", tableState_tableAlias, " WHERE ", tableState_tableAlias, ".", row_numberNameState, " > ", exp.SkipCount.ToString());

//            return sqlState;
//        }
//        SqlState BuildWhereState(DbExpression whereExpression)
//        {
//            SqlState whereState = null;
//            if (whereExpression != null)
//            {
//                whereState = new SqlState(2);
//                whereState.Append(" WHERE ", whereExpression.Accept(this));
//            }

//            return whereState;
//        }
//        SqlState BuildOrderState(List<DbOrderSegment> orderSegments)
//        {
//            if (orderSegments.Count > 0)
//            {
//                return SqlState.Create(" ORDER BY ", this.ConcatOrderSegments(orderSegments));
//            }

//            return null;
//        }
//        SqlState ConcatOrderSegments(List<DbOrderSegment> orderSegments)
//        {
//            SqlState state = new SqlState(orderSegments.Count + 1);

//            for (int i = 0; i < orderSegments.Count; i++)
//            {
//                if (i > 0)
//                {
//                    state.Append(",");
//                }

//                state.Append(this.AppendOrderSegment(orderSegments[i]));
//            }

//            return state;
//        }
//        SqlState BuildGroupState(DbSqlQueryExpression exp)
//        {
//            SqlState groupPartState = null;
//            var groupSegments = exp.GroupSegments;
//            groupPartState = new SqlState(2 + groupSegments.Count + (exp.HavingCondition != null ? 2 : 0));
//            groupPartState.Append(" GROUP BY ");

//            for (int i = 0; i < groupSegments.Count; i++)
//            {
//                if (i > 0)
//                    groupPartState.Append(",");

//                groupPartState.Append(groupSegments[i].Accept(this));
//            }

//            if (exp.HavingCondition != null)
//            {
//                groupPartState.Append(" HAVING ");
//                groupPartState.Append(exp.HavingCondition.Accept(this));
//            }

//            return groupPartState;
//        }

//        ISqlState ConcatOperands(Stack<DbExpression> operands, string connector)
//        {
//            SqlState state = new SqlState(2 + operands.Count + operands.Count - 1);
//            state.Append("(");

//            bool first = true;
//            foreach (DbExpression operand in operands)
//            {
//                if (first)
//                    first = false;
//                else
//                    state.Append(connector);

//                state.Append(operand.Accept(this));
//            }

//            state.Append(")");
//            return state;
//        }

//        static SqlState GetParameterNameSqlState(int ordinal, out string paramName)
//        {
//            if (ordinal <= CacheParameterSqlStates.Count - 1)
//            {
//                var tup = CacheParameterSqlStates[ordinal];
//                paramName = tup.Item1;
//                return tup.Item2;
//            }

//            paramName = ParameterPrefix + ordinal.ToString();
//            return SqlState.Create(paramName);
//        }
//        public static SqlState QuoteName(string name)
//        {
//            if (string.IsNullOrEmpty(name))
//                throw new ArgumentException("name");

//            return SqlState.Create("[", name, "]");
//        }
//        public static SqlState BracketState(ISqlState state)
//        {
//            return SqlState.Create("(", state, ")");
//        }

//        static SqlState BuildCastState(object castObject, string targetDbTypeString)
//        {
//            return SqlState.Create("CAST(", castObject, " AS ", targetDbTypeString, ")");
//        }
//        static string CreateRowNumberName(List<DbColumnSegment> columns)
//        {
//            int ROW_NUMBER_INDEX = 1;
//            string row_numberName = "ROW_NUMBER_0";
//            while (columns.Any(a => string.Equals(a.Alias, row_numberName, StringComparison.OrdinalIgnoreCase)))
//            {
//                row_numberName = "ROW_NUMBER_" + ROW_NUMBER_INDEX.ToString();
//                ROW_NUMBER_INDEX++;
//            }

//            return row_numberName;
//        }

//        static DbExpression EnsureDbExpressionReturnCSharpBoolean(DbExpression exp)
//        {
//            if (exp.Type != UtilConstants.TypeOfBoolean && exp.Type != UtilConstants.TypeOfBoolean_Nullable)
//                return exp;

//            if (SafeDbExpressionTypes.Contains(exp.NodeType))
//            {
//                return exp;
//            }

//            //将且认为不符合上述条件的都是诸如 a.Id>1,a.Name=="name" 等不能作为 bool 返回值的表达式
//            //构建 case when 
//            return ConstructReturnCSharpBooleanCaseWhenExpression(exp);
//        }
//        public static DbCaseWhenExpression ConstructReturnCSharpBooleanCaseWhenExpression(DbExpression exp)
//        {
//            // case when 1>0 then 1 when not (1>0) then 0 else Null end
//            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(exp, DbConstantExpression.True);
//            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair1 = new DbCaseWhenExpression.WhenThenExpressionPair(DbExpression.Not(exp), DbConstantExpression.False);
//            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(2);
//            whenThenExps.Add(whenThenPair);
//            whenThenExps.Add(whenThenPair1);
//            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Null, UtilConstants.TypeOfBoolean);

//            return caseWhenExpression;
//        }
//        static Stack<DbExpression> GatherBinaryExpressionOperand(DbBinaryExpression exp)
//        {
//            DbExpressionType nodeType = exp.NodeType;

//            Stack<DbExpression> items = new Stack<DbExpression>();
//            items.Push(exp.Right);

//            DbExpression left = exp.Left;
//            while (left.NodeType == nodeType)
//            {
//                exp = (DbBinaryExpression)left;
//                items.Push(exp.Right);
//                left = exp.Left;
//            }

//            items.Push(left);
//            return items;
//        }
//        static void EnsureMethodDeclaringType(DbMethodCallExpression exp, Type ensureType)
//        {
//            if (exp.Method.DeclaringType != ensureType)
//                throw UtilExceptions.NotSupportedMethod(exp.Method);
//        }
//        static void EnsureMethod(DbMethodCallExpression exp, MethodInfo methodInfo)
//        {
//            if (exp.Method != methodInfo)
//                throw UtilExceptions.NotSupportedMethod(exp.Method);
//        }


//        #region BinaryWithMethodHandlers

//        static Dictionary<MethodInfo, Func<DbBinaryExpression, SqlExpressionVisitor, ISqlState>> InitBinaryWithMethodHandlers()
//        {
//            var binaryWithMethodHandlers = new Dictionary<MethodInfo, Func<DbBinaryExpression, SqlExpressionVisitor, ISqlState>>();
//            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_String_String, StringConcat);
//            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_Object_Object, StringConcat);

//            var ret = new Dictionary<MethodInfo, Func<DbBinaryExpression, SqlExpressionVisitor, ISqlState>>(binaryWithMethodHandlers.Count);
//            foreach (var item in binaryWithMethodHandlers)
//            {
//                ret.Add(item.Key, item.Value);
//            }

//            return ret;
//        }

//        static ISqlState StringConcat(DbBinaryExpression exp, SqlExpressionVisitor visitor)
//        {
//            MethodInfo method = exp.Method;

//            List<DbExpression> operands = new List<DbExpression>();
//            operands.Add(exp.Right);

//            DbExpression left = exp.Left;
//            DbAddExpression e = null;
//            while ((e = (left as DbAddExpression)) != null && (e.Method == UtilConstants.MethodInfo_String_Concat_String_String || e.Method == UtilConstants.MethodInfo_String_Concat_Object_Object))
//            {
//                operands.Add(e.Right);
//                left = e.Left;
//            }

//            operands.Add(left);

//            SqlState state = new SqlState(3 + operands.Count);

//            DbExpression whenExp = null;

//            state.Append("(");
//            for (int i = operands.Count - 1; i >= 0; i--)
//            {
//                DbExpression operand = operands[i];
//                DbExpression opBody = operand;
//                if (opBody.Type != UtilConstants.TypeOfString)
//                {
//                    // 需要 cast type
//                    opBody = DbExpression.Convert(opBody, UtilConstants.TypeOfString);
//                }

//                DbExpression equalNullExp = DbExpression.Equal(opBody, UtilConstants.DbConstant_Null_String);

//                if (whenExp == null)
//                    whenExp = equalNullExp;
//                else
//                    whenExp = DbExpression.AndAlso(whenExp, equalNullExp);

//                DbExpression thenExp = DbExpression.Constant("");
//                DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(equalNullExp, thenExp);

//                List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
//                whenThenExps.Add(whenThenPair);

//                DbExpression elseExp = opBody;

//                DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, elseExp, UtilConstants.TypeOfString);

//                if (i < operands.Count - 1)
//                    state.Append(" + ");
//                state.Append(caseWhenExpression.Accept(visitor));
//            }
//            state.Append(")");

//            SqlState retState = new SqlState(8);
//            retState.Append("CASE", " WHEN ", whenExp.Accept(visitor), " THEN ", DbConstantExpression.Null.Accept(visitor));
//            retState.Append(" ELSE ", state, " END");

//            return retState;
//        }

//        #endregion

//        #region MethodHandlers

//        static Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>> InitMethodHandlers()
//        {
//            var methodHandlers = new Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>>();
//            methodHandlers.Add("Trim", Method_Trim);
//            methodHandlers.Add("TrimStart", Method_TrimStart);
//            methodHandlers.Add("TrimEnd", Method_TrimEnd);
//            methodHandlers.Add("StartsWith", Method_StartsWith);
//            methodHandlers.Add("EndsWith", Method_EndsWith);
//            methodHandlers.Add("ToUpper", Method_String_ToUpper);
//            methodHandlers.Add("ToLower", Method_String_ToLower);
//            methodHandlers.Add("Substring", Method_String_Substring);
//            methodHandlers.Add("IsNullOrEmpty", Method_String_IsNullOrEmpty);

//            methodHandlers.Add("Contains", Method_Contains);

//            methodHandlers.Add("Count", Method_Count);
//            methodHandlers.Add("LongCount", Method_LongCount);
//            methodHandlers.Add("Sum", Method_Sum);
//            methodHandlers.Add("Max", Method_Max);
//            methodHandlers.Add("Min", Method_Min);
//            methodHandlers.Add("Average", Method_Average);

//            methodHandlers.Add("AddYears", Method_DateTime_AddYears);
//            methodHandlers.Add("AddMonths", Method_DateTime_AddMonths);
//            methodHandlers.Add("AddDays", Method_DateTime_AddDays);
//            methodHandlers.Add("AddHours", Method_DateTime_AddHours);
//            methodHandlers.Add("AddMinutes", Method_DateTime_AddMinutes);
//            methodHandlers.Add("AddSeconds", Method_DateTime_AddSeconds);
//            methodHandlers.Add("AddMilliseconds", Method_DateTime_AddMilliseconds);

//            methodHandlers.Add("Parse", Method_Parse);

//            methodHandlers.Add("NewGuid", Method_Guid_NewGuid);

//            var ret = new Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>>(methodHandlers.Count, StringComparer.Ordinal);
//            foreach (var item in methodHandlers)
//            {
//                ret.Add(item.Key, item.Value);
//            }

//            return ret;
//        }

//        static ISqlState Method_Trim(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_Trim);
//            return SqlState.Create("RTRIM(LTRIM(", exp.Object.Accept(visitor), "))");
//        }
//        static ISqlState Method_TrimStart(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimStart);
//            EnsureTrimCharParameterIsSpaces(exp.Arguments[0]);
//            return SqlState.Create("LTRIM(", exp.Object.Accept(visitor), ")");
//        }
//        static ISqlState Method_TrimEnd(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimEnd);
//            EnsureTrimCharParameterIsSpaces(exp.Arguments[0]);
//            return SqlState.Create("RTRIM(", exp.Object.Accept(visitor), ")");
//        }
//        static ISqlState Method_StartsWith(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_StartsWith);
//            return SqlState.Create(exp.Object.Accept(visitor), " LIKE ", exp.Arguments.First().Accept(visitor), " + '%'");
//        }
//        static ISqlState Method_EndsWith(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_EndsWith);
//            return SqlState.Create(exp.Object.Accept(visitor), " LIKE '%' + ", exp.Arguments.First().Accept(visitor));
//        }
//        static ISqlState Method_String_Contains(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_Contains);
//            return SqlState.Create(exp.Object.Accept(visitor), " LIKE '%' + ", exp.Arguments.First().Accept(visitor), " + '%'");
//        }
//        static ISqlState Method_String_ToUpper(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToUpper);
//            return SqlState.Create("UPPER(", exp.Object.Accept(visitor), ")");
//        }
//        static ISqlState Method_String_ToLower(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToLower);
//            return SqlState.Create("LOWER(", exp.Object.Accept(visitor), ")");
//        }
//        static ISqlState Method_String_Substring(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            ISqlState length = null;

//            if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32)
//            {
//                var string_LengthExp = DbExpression.MemberAccess(UtilConstants.PropertyInfo_String_Length, exp.Object);
//                length = string_LengthExp.Accept(visitor);
//            }
//            else if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32_Int32)
//            {
//                length = exp.Arguments[1].Accept(visitor);
//            }
//            else
//                throw UtilExceptions.NotSupportedMethod(exp.Method);

//            return SqlState.Create("SUBSTRING(", exp.Object.Accept(visitor), ",", exp.Arguments[0].Accept(visitor), " + 1", ",", length, ")");
//        }
//        static ISqlState Method_String_IsNullOrEmpty(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_String_IsNullOrEmpty);

//            DbExpression e = exp.Arguments.First();
//            DbEqualExpression equalNullExpression = DbExpression.Equal(e, DbExpression.Constant(null, UtilConstants.TypeOfString));
//            DbEqualExpression equalEmptyExpression = DbExpression.Equal(e, DbExpression.Constant(string.Empty));

//            DbOrElseExpression orElseExpression = DbExpression.OrElse(equalNullExpression, equalEmptyExpression);

//            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(orElseExpression, DbConstantExpression.One);

//            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
//            whenThenExps.Add(whenThenPair);

//            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Zero, UtilConstants.TypeOfBoolean);

//            var eqExp = DbExpression.Equal(caseWhenExpression, DbConstantExpression.One);
//            return eqExp.Accept(visitor);
//        }

//        static ISqlState Method_Contains(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            MethodInfo method = exp.Method;

//            if (method.DeclaringType == UtilConstants.TypeOfString)
//                return Method_String_Contains(exp, visitor);

//            List<DbExpression> exps = new List<DbExpression>();
//            IEnumerable values = null;
//            DbExpression arg = null;

//            var declaringType = method.DeclaringType;

//            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GenericTypeArguments).IsAssignableFrom(declaringType)))
//            {
//                DbMemberExpression memberExp = exp.Object as DbMemberExpression;

//                if (memberExp == null || !memberExp.CanEvaluate())
//                    throw new NotSupportedException(exp.Object.ToString());

//                values = DbExpressionExtensions.GetExpressionValue(memberExp) as IEnumerable; //Enumerable
//                arg = exp.Arguments.First();
//                goto constructInState;
//            }
//            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
//            {
//                DbMemberExpression memberExp = exp.Arguments.First() as DbMemberExpression;

//                if (memberExp == null || !memberExp.CanEvaluate())
//                    throw new NotSupportedException(exp.Object.ToString());

//                values = DbExpressionExtensions.GetExpressionValue(memberExp) as IEnumerable;
//                arg = exp.Arguments.Skip(1).First();
//                goto constructInState;
//            }

//            throw new NotSupportedException(exp.Object.ToString());

//        constructInState:
//            foreach (object value in values)
//            {
//                if (value == null)
//                    exps.Add(DbExpression.Constant(null, arg.Type));
//                else
//                    exps.Add(DbExpression.Parameter(value));
//            }
//            return In(visitor, exps, arg);
//        }


//        static ISqlState In(DbExpressionVisitor<ISqlState> visitor, List<DbExpression> elementExps, DbExpression arg)
//        {
//            SqlState state = null;

//            if (elementExps.Count == 0)
//            {
//                return SqlState.Create("1=0");
//            }

//            state = new SqlState((elementExps.Count == 1 ? 3 : 4) + elementExps.Count);
//            state.Append(arg.Accept(visitor));
//            state.Append(" IN (");

//            var first = true;
//            foreach (DbExpression ele in elementExps)
//            {
//                if (first)
//                    first = false;
//                else
//                    state.Append(",");

//                state.Append(ele.Accept(visitor));
//            }

//            state.Append(")");

//            return state;
//        }

//        static ISqlState Method_Count(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Count();
//        }
//        static ISqlState Method_LongCount(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_LongCount();
//        }
//        static ISqlState Method_Sum(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Sum(exp.Arguments.First(), visitor);
//        }
//        static ISqlState Method_Max(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Max(exp.Arguments.First(), visitor);
//        }
//        static ISqlState Method_Min(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Min(exp.Arguments.First(), visitor);
//        }
//        static ISqlState Method_Average(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            ISqlState state = Func_Average(exp.Arguments.First(), visitor);

//            string dbTypeString;
//            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
//            {
//                state = BuildCastState(state, dbTypeString);
//            }

//            return state;
//        }


//        static ISqlState Method_DateTime_AddYears(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("YEAR", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddMonths(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("MONTH", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddDays(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("DAY", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddHours(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("HOUR", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddMinutes(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("MINUTE", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddSeconds(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("SECOND", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }
//        static ISqlState Method_DateTime_AddMilliseconds(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);
//            return DbFunction_DATEADD("MILLISECOND", exp.Arguments[0].Accept(visitor), exp.Object.Accept(visitor));
//        }

//        static ISqlState Method_Parse(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            if (exp.Arguments.Count != 1)
//                throw new NotSupportedException();

//            DbExpression arg = exp.Arguments[0];
//            if (arg.Type != UtilConstants.TypeOfString)
//                throw new NotSupportedException();

//            Type retType = exp.Method.ReturnType;
//            EnsureMethodDeclaringType(exp, retType);

//            DbExpression e = DbExpression.Convert(arg, retType);
//            if (retType == UtilConstants.TypeOfBoolean)
//            {
//                return SqlState.Create(e.Accept(visitor), " = ", DbConstantExpression.True.Accept(visitor));
//            }

//            return e.Accept(visitor);
//        }

//        static ISqlState Method_Guid_NewGuid(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
//        {
//            EnsureMethod(exp, UtilConstants.MethodInfo_Guid_NewGuid);
//            return SqlState.Create("NEWID()");
//        }

//        #endregion

//        #region FuncHandlers
//        static Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>> InitFuncHandlers()
//        {
//            var funcHandlers = new Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>>();
//            funcHandlers.Add("Count", Func_Count);
//            funcHandlers.Add("LongCount", Func_LongCount);
//            funcHandlers.Add("Sum", Func_Sum);
//            funcHandlers.Add("Max", Func_Max);
//            funcHandlers.Add("Min", Func_Min);
//            funcHandlers.Add("Average", Func_Average);

//            var ret = new Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>>(funcHandlers.Count, StringComparer.Ordinal);
//            foreach (var item in funcHandlers)
//            {
//                ret.Add(item.Key, item.Value);
//            }

//            return ret;
//        }

//        static ISqlState Func_Count(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Count();
//        }
//        static ISqlState Func_LongCount(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_LongCount();
//        }
//        static ISqlState Func_Sum(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Sum(exp.Parameters.First(), visitor);
//        }
//        static ISqlState Func_Max(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Max(exp.Parameters.First(), visitor);
//        }
//        static ISqlState Func_Min(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            return Func_Min(exp.Parameters.First(), visitor);
//        }
//        static ISqlState Func_Average(DbFunctionExpression exp, SqlExpressionVisitor visitor)
//        {
//            ISqlState state = Func_Average(exp.Parameters.First(), visitor);

//            string dbTypeString;
//            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
//            {
//                state = BuildCastState(state, dbTypeString);
//            }

//            return state;
//        }

//        #endregion

//        #region AggregateFunction
//        static ISqlState Func_Count()
//        {
//            return SqlState.Create("COUNT(1)");
//        }
//        static ISqlState Func_LongCount()
//        {
//            return SqlState.Create("COUNT_BIG(1)");
//        }
//        static ISqlState Func_Sum(DbExpression exp, SqlExpressionVisitor visitor)
//        {
//            return SqlState.Create("SUM(", exp.Accept(visitor), ")");
//        }
//        static ISqlState Func_Max(DbExpression exp, SqlExpressionVisitor visitor)
//        {
//            return SqlState.Create("MAX(", exp.Accept(visitor), ")");
//        }
//        static ISqlState Func_Min(DbExpression exp, SqlExpressionVisitor visitor)
//        {
//            return SqlState.Create("MIN(", exp.Accept(visitor), ")");
//        }
//        static ISqlState Func_Average(DbExpression exp, SqlExpressionVisitor visitor)
//        {
//            return SqlState.Create("AVG(", exp.Accept(visitor), ")");
//        }
//        #endregion


//        static ISqlState DbFunction_DATEADD(string interval, ISqlState incrementSqlState, ISqlState dateTimeSqlState)
//        {
//            return SqlState.Create("DATEADD(", interval, ",", incrementSqlState, ",", dateTimeSqlState, ")");
//        }
//        static ISqlState DbFunction_DATEPART(string interval, ISqlState sqlState)
//        {
//            return SqlState.Create("DATEPART(", interval, ",", sqlState, ")");
//        }
//        static ISqlState DbFunction_DATEDIFF(string interval, ISqlState startDateTime, ISqlState endDateTime)
//        {
//            return SqlState.Create("DATEDIFF(", interval, ",", startDateTime, ",", endDateTime, ")");
//        }

//        static bool IsDbFunction_DATEPART(DbMemberExpression exp, SqlExpressionVisitor visitor, out ISqlState sqlState)
//        {
//            sqlState = null;

//            MemberInfo member = exp.Member;

//            if (member == UtilConstants.PropertyInfo_DateTime_Year)
//            {
//                sqlState = DbFunction_DATEPART("YEAR", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Month)
//            {
//                sqlState = DbFunction_DATEPART("MONTH", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Day)
//            {
//                sqlState = DbFunction_DATEPART("DAY", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Hour)
//            {
//                sqlState = DbFunction_DATEPART("HOUR", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Minute)
//            {
//                sqlState = DbFunction_DATEPART("MINUTE", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Second)
//            {
//                sqlState = DbFunction_DATEPART("SECOND", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_Millisecond)
//            {
//                sqlState = DbFunction_DATEPART("MILLISECOND", exp.Expression.Accept(visitor));
//                return true;
//            }

//            if (member == UtilConstants.PropertyInfo_DateTime_DayOfWeek)
//            {
//                sqlState = SqlState.Create("(", DbFunction_DATEPART("WEEKDAY", exp.Expression.Accept(visitor)), " - 1)");
//                return true;
//            }

//            return false;
//        }
//        static bool IsDbFunction_DATEDIFF(DbMemberExpression exp, SqlExpressionVisitor visitor, out ISqlState sqlState)
//        {
//            sqlState = null;

//            MemberInfo member = exp.Member;

//            if (member.DeclaringType == UtilConstants.TypeOfTimeSpan)
//            {
//                if (exp.Expression.NodeType == DbExpressionType.Call)
//                {
//                    DbMethodCallExpression dbMethodExp = (DbMethodCallExpression)exp.Expression;
//                    if (dbMethodExp.Method == UtilConstants.MethodInfo_DateTime_Subtract_DateTime)
//                    {
//                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalDays)
//                        {
//                            sqlState = BuildCastState(DbFunction_DATEDIFF("DAY", dbMethodExp.Arguments[0].Accept(visitor), dbMethodExp.Object.Accept(visitor)), "FLOAT");
//                            return true;
//                        }

//                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalHours)
//                        {
//                            sqlState = BuildCastState(DbFunction_DATEDIFF("HOUR", dbMethodExp.Arguments[0].Accept(visitor), dbMethodExp.Object.Accept(visitor)), "FLOAT");
//                            return true;
//                        }

//                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMinutes)
//                        {
//                            sqlState = BuildCastState(DbFunction_DATEDIFF("MINUTE", dbMethodExp.Arguments[0].Accept(visitor), dbMethodExp.Object.Accept(visitor)), "FLOAT");
//                            return true;
//                        }

//                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalSeconds)
//                        {
//                            sqlState = BuildCastState(DbFunction_DATEDIFF("SECOND", dbMethodExp.Arguments[0].Accept(visitor), dbMethodExp.Object.Accept(visitor)), "FLOAT");
//                            return true;
//                        }

//                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMilliseconds)
//                        {
//                            sqlState = BuildCastState(DbFunction_DATEDIFF("MILLISECOND", dbMethodExp.Arguments[0].Accept(visitor), dbMethodExp.Object.Accept(visitor)), "FLOAT");
//                            return true;
//                        }
//                    }
//                }
//            }

//            return false;
//        }

//        static void EnsureTrimCharParameterIsSpaces(DbExpression exp)
//        {
//            var m = exp as DbMemberExpression;
//            if (m == null)
//                throw new NotSupportedException();

//            DbParameterExpression p;
//            if (!DbExpressionExtensions.TryParseToParameterExpression(m, out p))
//            {
//                throw new NotSupportedException();
//            }

//            var arg = p.Value;

//            if (arg == null)
//                throw new NotSupportedException();

//            var chars = arg as char[];
//            if (chars.Length != 1 || chars[0] != ' ')
//            {
//                throw new NotSupportedException();
//            }
//        }
//    }

//}
//#endif